using esper.plugins;
using System.Collections.Generic;
using System.IO;
using balsa.setup;
using System.Linq;
using balsa.stringtables;

namespace esper.setup {
    public class ResourceManager {
        private readonly Session session;
        private readonly List<string> allArchives;
        private readonly balsa.Game balsaGame;

        public readonly AssetManager assetManager;

        // TOOD: load archives from INI file
        public ResourceManager(Session session) {
            this.session = session;
            balsaGame = ResolveBalsaGame();
            assetManager = new AssetManager(balsaGame);
            allArchives = GetAllArchives();
        }

        private balsa.Game ResolveBalsaGame() {
            var targetGameKey = session.game.abbreviation;
            var fields = typeof(balsa.Games).GetFields();
            foreach (var field in fields)
                if (field.Name == targetGameKey)
                    return (balsa.Game)field.GetValue(null);
            return null;
        }

        private List<string> GetAllArchives() {
            var searchPath = session.dataPath;
            var search = $"*{session.game.archiveExtension}";
            return Directory.GetFiles(searchPath, search).ToList();
        }

        private string GetBaseName(string path) {
            return Path.GetFileNameWithoutExtension(path);
        }

        public List<string> GetAssociatedArchives(string pluginName) {
            var baseName = GetBaseName(pluginName);
            return allArchives.FindAll(archivePath => {
                var archiveName = GetBaseName(archivePath);
                return session.game.extendedArchiveMatching
                    ? archiveName.StartsWith(baseName)
                    : archiveName == baseName;
            });
        }

        public void LoadAssociatedArchives(PluginFile plugin) {
            var archivesToLoad = GetAssociatedArchives(plugin.name);
            foreach (string archivePath in archivesToLoad)
                assetManager.LoadArchive(archivePath);
        }

        public void LoadArchives() {
            foreach (var plugin in session.pluginManager.plugins)
                LoadAssociatedArchives(plugin);
        }

        public void LoadData() {
            assetManager.LoadFolder(session.dataPath);
        }

        public void LoadAssociatedStrings(string filePath, PluginFile plugin) {
            var stringFile = assetManager.LoadStrings(filePath);
            plugin.stringFiles.Add(stringFile);
        }

        public void LoadStrings() {
            var stringTables = assetManager.GetStringTables();
            var languageSuffix = session.options.language.ToLower();
            foreach (var plugin in session.pluginManager.plugins) {
                if (!plugin.localized) continue;
                var key = $"{plugin.name}_{languageSuffix}";
                if (!stringTables.ContainsKey(key)) continue;
                var stringFilePaths = stringTables[key];
                plugin.stringFiles = new List<StringFile>();
                foreach (string filePath in stringFilePaths)
                    LoadAssociatedStrings(filePath, plugin);
            }
        }
    }
}
