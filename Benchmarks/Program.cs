using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using esper;
using esper.plugins;
using esper.setup;
using esper.resolution;

namespace Benchmarks {
    class Program {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public ResourceManager resourceManager;
        public PluginFile plugin;

        public static string fixturesPath = Path.Combine(
            Environment.CurrentDirectory,
            "fixtures"
        );

        public static string FixturePath(string filename) {
            return Path.Combine(fixturesPath, filename);
        }

        private List<string> GetStringFiles() {
            var skyrimStringsPath = FixturePath("skyrim_strings");
            var stringFiles = Directory.GetFiles(
                skyrimStringsPath, "skyrim_english.*"
            ).ToList();
            return stringFiles;
        }

        private void LoadSkyrimEsm() {
            var pluginPath = FixturePath("Skyrim.esm");
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions());
            resourceManager = new ResourceManager(session, true);
            LoadSkyrimEsm();
            resourceManager.LoadPluginStrings(plugin, GetStringFiles());
        }

        public void TouchSubrecords(string signature, string path) {
            var records = plugin.GetElements(signature);
            foreach (var record in records)
                record.GetValue(path);
        }

        static void Main(string[] args) {
            var p = new Program();
            p.SetUp();
            p.TouchSubrecords("WEAP", "FULL");
            p.TouchSubrecords("NPC_", "FULL");
        }
    }
}
