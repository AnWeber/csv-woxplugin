using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wox.Plugin;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace csv.woxplugin
{
    /// <summary>
    /// Wox Plugin
    /// </summary>
    public class Plugin : Wox.Plugin.IPlugin {

        private List<Result> Results;

        public string Name => "CSV Plugin";

        public string Description => "CSV plugin for extending the search with arbitrary values";

        public void Init(PluginInitContext context) {
            this.Results = new List<Result>();

            foreach(var file in Directory.GetFiles(context.CurrentPluginMetadata.PluginDirectory, "*.csv")) {
                this.Results.AddRange(ReadCSVFile(file));
            }
        }

        private ICollection<Result> ReadCSVFile(string file, string icoPath = "csv.png") {
            var results = new List<Result>();
            if (File.Exists(file)) {
                foreach (var line in File.ReadAllLines(file, Encoding.Default)) {

                    if (!string.IsNullOrEmpty(line.Trim())) {
                        var cols = line.Split(';');
                        var result = new Result {
                            Title = line,
                            Score = 100,
                            IcoPath = icoPath,
                        };

                        if (cols.Length >= 1) {
                            string key = cols[0];
                            string value = cols[1];

                            if (cols.Length > 2) {
                                key = string.Format("{0} ({1})", key, cols.Skip(2).Aggregate((workingSentence, next) => string.Format("{0}, {1}", workingSentence, next)));
                            }

                            result.Title = key;
                            result.SubTitle = value;

                            result.Action = (ctx) => {
                                Clipboard.SetText(result.SubTitle);
                                return true;
                            };
                        }
                        results.Add(result);
                    }

                }
            }
            return results;
        }

        /// <summary>
        /// Ausführung einer Query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<Result> Query(Query query) {

            return Results
                .Where(obj => obj.Title.ToLower().Contains(query.Search.ToLower()) || (!string.IsNullOrEmpty(obj.SubTitle) && obj.SubTitle.ToLower().Contains(query.Search.ToLower())))
                .OrderBy(obj => obj.Title)
                .ToList();
        }
    }
}
