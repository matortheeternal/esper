using esper.helpers;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;

namespace esper {
    [JSExport]
    public class Game {
        public int xeditId { get; internal set; }
        public string name { get; internal set; }
        public string baseName { get; internal set; }
        public string fullName { get; internal set; }
        public string abbreviation { get; internal set; }
        public string defsNamespace { get; internal set; }
        public string registryName { get; internal set; }
        public string myGamesFolderName { get; internal set; }
        public string appDataFolderName { get; internal set; }
        public string exeName { get; internal set; }
        public string esmName { get; internal set; }
        public string iniName { get; internal set; }
        public string headerTypeKey { get; internal set; } = "TES4";
        public string cccName { get; internal set; } = null;
        public PluginsTxtType pluginsTxtType { get; internal set; } = PluginsTxtType.Plain;
        public string archiveExtension { get; internal set; } = ".bsa";
        internal UInt32 recordHeaderSize { get; set; } = 24;
        internal UInt32 groupHeaderSize { get; set; } = 24;
        internal RegistryHive registryRoot = RegistryHive.LocalMachine;
        public string registryValue { get; internal set; } = "Installed Path";
        public string registryPath { get; internal set; } = @"SOFTWARE\Bethesda Softworks";
        public bool extendedArchiveMatching { get; internal set; } = false;
        public List<ModuleExtension> pluginExtensions { get; internal set; } = new List<ModuleExtension> {
            ModuleExtension.ESP, ModuleExtension.ESM
        };
        public List<string> hardcodedPlugins { get; internal set; } = new List<string>();
        public List<int> steamAppIds { get; internal set; } = new List<int>();

        public string myGamesPath {
            get {
                var docsId = Environment.SpecialFolder.MyDocuments;
                var docsPath = Environment.GetFolderPath(docsId);
                return Path.Combine(docsPath, "My Games", myGamesFolderName);
            }
        }

        public string appDataPath {
            get {
                var appDataId = Environment.SpecialFolder.LocalApplicationData;
                var appDataPath = Environment.GetFolderPath(appDataId);
                return Path.Combine(appDataPath, appDataFolderName);
            }
        }

        private IniData _iniData;
        internal IniData gameIni {
            get {
                if (_iniData == null) {
                    var parser = new FileIniDataParser();
                    _iniData = parser.ReadFile(iniPath);
                }
                return _iniData;
            }
        }

        public string iniPath => Path.Combine(myGamesPath, iniName);

        public Game(
            string nameOverride = null, string baseNameOverride = null
        ) {
            if (nameOverride != null) {
                registryName = nameOverride;
                myGamesFolderName = nameOverride;
                appDataFolderName = nameOverride;
            }
            if (baseNameOverride != null) {
                exeName = $"{baseNameOverride}.exe";
                esmName = $"{baseNameOverride}.esm";
                iniName = $"{baseNameOverride}.ini";
            }
            hardcodedPlugins.Insert(0, esmName);
        }

        public Game InitDefaults() {
            if (baseName == null) baseName = name.Replace(" ", string.Empty);
            if (fullName == null) fullName = name;
            if (defsNamespace == null) defsNamespace = abbreviation;
            if (registryName == null) registryName = name;
            if (myGamesFolderName == null) myGamesFolderName = name;
            if (appDataFolderName == null) appDataFolderName = name;
            if (exeName == null) exeName = $"{baseName}.exe";
            if (esmName == null) esmName = $"{baseName}.esm";
            if (iniName == null) iniName = $"{baseName}.ini";
            // The constructor inserts esmName at index 0 before object
            // initializers run, so it is null unless baseNameOverride was
            // given. Repair that here now that esmName is final.
            if (hardcodedPlugins.Count == 0) hardcodedPlugins.Add(esmName);
            else if (hardcodedPlugins[0] == null) hardcodedPlugins[0] = esmName;
            else if (hardcodedPlugins[0] != esmName) hardcodedPlugins.Insert(0, esmName);
            return this;
        }

        public bool SupportsLightPlugins() {
            return pluginExtensions.Contains(ModuleExtension.ESL);
        }

        public string GetInstallLocation() {
            var keyPath = $@"{registryPath}\{registryName}";
            return RegistryWOW6432.TryGetRegKey(registryRoot, keyPath, registryValue);
        }
    }
}
