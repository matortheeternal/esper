using esper;
using esper.plugins;
using esper.setup;
using System;
using System.Diagnostics;
using System.IO;


namespace Fallout4 {
    class Program {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public DefinitionManager definitionManager => session.definitionManager;
        public PluginFile plugin;

        public static string fixturesPath = Path.Combine(
            Environment.CurrentDirectory,
            "fixtures"
        );

        public static string FixturePath(string filename) {
            return Path.Combine(fixturesPath, filename);
        }

        private void LoadFallout4Esm() {
            var pluginPath = FixturePath("Fallout4.esm");
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        public void SetUp() {
            session = new Session(Games.FO4, new SessionOptions());
            LoadFallout4Esm();
        }

        public void ExportDefinitions() {
            definitionManager.ExportRecordSignatureList();
            Console.WriteLine($"Definitions exported successfully.");
        }

        static void Main(string[] args) {
            var watch = new Stopwatch();
            var p = new Program();
            watch.Start();
            p.SetUp();
            watch.Stop();
            Console.WriteLine($"{watch.ElapsedMilliseconds}ms spent setting up.");
            Console.WriteLine($"Exporting definitions.");
            p.ExportDefinitions();
        }
    }
}
