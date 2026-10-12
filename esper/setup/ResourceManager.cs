using balsa.setup;
using balsa.stringtables;
using esper.plugins;

namespace esper.setup {
    [JSExport]
    public class ResourceManager {
        private readonly Session session;
        private List<string> allArchives = new List<string>();
        private readonly balsa.Game balsaGame;
        private GameIni gameIni => session.gameIni;

        public readonly AssetManager assetManager;

        public ResourceManager(Session session) {
            this.session = session;
            balsaGame = ResolveBalsaGame();
            if (balsaGame != null) assetManager = new AssetManager(balsaGame);
        }

        internal void Init() {
            EnsureAssetManager();
            allArchives = GetAllArchives();
        }

        private AssetManager EnsureAssetManager() {
            if (assetManager == null) {
                throw new NotSupportedException(
                    $"Resource loading is not supported for {session.game.name}."
                );
            }
            return assetManager;
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
            if (string.IsNullOrWhiteSpace(searchPath)) {
                session.logger.Warn("Cannot find archives without a data path.");
                return new List<string>();
            }
            if (!Directory.Exists(searchPath)) {
                session.logger.Warn($"Data path not found: {searchPath}");
                return new List<string>();
            }
            return Directory.EnumerateFiles(searchPath)
                .Where(path => string.Equals(
                    Path.GetExtension(path), session.game.archiveExtension,
                    StringComparison.OrdinalIgnoreCase
                ))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string GetBaseName(string path) {
            return Path.GetFileNameWithoutExtension(path);
        }

        public List<string> GetAssociatedArchives(string pluginName) {
            var baseName = GetBaseName(pluginName);
            return allArchives.FindAll(archivePath => {
                var archiveName = GetBaseName(archivePath);
                return session.game.extendedArchiveMatching
                    ? archiveName.StartsWith(
                        baseName, StringComparison.OrdinalIgnoreCase
                    )
                    : string.Equals(
                        archiveName, baseName,
                        StringComparison.OrdinalIgnoreCase
                    );
            });
        }

        public void LoadAssociatedArchives(PluginFile plugin) {
            var manager = EnsureAssetManager();
            var archivesToLoad = GetAssociatedArchives(plugin.name);
            foreach (string archivePath in archivesToLoad)
                manager.LoadArchive(archivePath);
        }

        private void LoadIniArchives() {
            var manager = EnsureAssetManager();
            var iniArchives = gameIni.GetArchives();
            if (iniArchives.Count == 0) return;
            if (string.IsNullOrWhiteSpace(session.dataPath) ||
                !Directory.Exists(session.dataPath)) {
                session.logger.Warn($"Data path not found: {session.dataPath}");
                return;
            }
            foreach (string archiveFileName in iniArchives) {
                var relativePath = archiveFileName
                    .Replace('\\', Path.DirectorySeparatorChar)
                    .Replace('/', Path.DirectorySeparatorChar);
                var archivePath = Path.Combine(session.dataPath, relativePath);
                if (!File.Exists(archivePath)) {
                    session.logger.Warn($"Archive not found: {archivePath}");
                    continue;
                }
                manager.LoadArchive(archivePath);
            }
        }

        public void LoadArchives() {
            LoadIniArchives();
            foreach (var plugin in session.pluginManager.plugins)
                LoadAssociatedArchives(plugin);
        }

        public void LoadData() {
            var manager = EnsureAssetManager();
            if (string.IsNullOrWhiteSpace(session.dataPath) ||
                !Directory.Exists(session.dataPath)) {
                session.logger.Warn($"Data path not found: {session.dataPath}");
                return;
            }
            manager.LoadFolder(session.dataPath);
        }

        public void LoadPluginStrings(PluginFile plugin, List<string> stringFilePaths) {
            var manager = EnsureAssetManager();
            plugin.stringFiles = new Dictionary<StringFileType, StringFile>();
            foreach (string filePath in stringFilePaths) {
                var stringFile = manager.LoadStrings(filePath);
                plugin.stringFiles.Add(stringFile.stringFileType, stringFile);
            }
        }

        public void LoadStrings() {
            var stringTables = EnsureAssetManager().GetStringTables();
            foreach (var plugin in session.pluginManager.plugins) {
                if (!plugin.localized) continue;
                var key = GetStringTableKey(
                    plugin.name, session.options.language
                );
                var stringFiles = stringTables.FirstOrDefault(entry =>
                    string.Equals(
                        entry.Key, key, StringComparison.OrdinalIgnoreCase
                    )
                ).Value;
                if (stringFiles == null) continue;
                LoadPluginStrings(plugin, stringFiles);
            }
        }

        internal static string GetStringTableKey(
            string pluginName, string language
        ) {
            var pluginBaseName = Path.GetFileNameWithoutExtension(pluginName);
            return $"{pluginBaseName}_{language}".ToLowerInvariant();
        }
    }
}
