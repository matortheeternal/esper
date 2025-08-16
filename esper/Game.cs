using esper.helpers;
using IniParser;
using IniParser.Model;

namespace esper {
    [JSExport]
    public class Game {
        public int xeditId;
        public string name;
        public string baseName;
        public string fullName;
        public string abbreviation;
        public string defsNamespace;
        public string registryName;
        public string myGamesFolderName;
        public string appDataFolderName;
        public string exeName;
        public string esmName;
        public string iniName;
        public string headerTypeKey = "TES4";
        public string cccName = null;
        public PluginsTxtType pluginsTxtType = PluginsTxtType.Plain;
        public string archiveExtension = ".bsa";
        public UIntPtr registryRoot = RegHive.HKEY_LOCAL_MACHINE;
        public string registryValue = "Installed Path";
        public string registryPath = @"SOFTWARE\Bethesda Softworks";
        public bool extendedArchiveMatching = false;
        public List<ModuleExtension> pluginExtensions = new List<ModuleExtension> {
            ModuleExtension.ESP, ModuleExtension.ESM
        };
        public List<string> hardcodedPlugins = new List<string>();
        public List<int> steamAppIds = new List<int>();

        public string myGamesPath {
            get {
                var docsId = Environment.SpecialFolder.MyDocuments;
                var docsPath = Environment.GetFolderPath(docsId);
                return Path.Combine(docsPath, "My Games", myGamesFolderName);
            }
        }

        private IniData _iniData;
        public IniData gameIni {
            get {
                if (_iniData == null) {
                    var parser = new FileIniDataParser();
                    _iniData = parser.ReadFile(iniPath);
                }
                return _iniData;
            }
        }

        public string iniPath => Path.Combine(myGamesPath, $"{iniName}.ini");

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
            return this;
        }

        public bool SupportsLightPlugins() {
            return pluginExtensions.Contains(ModuleExtension.ESL);
        }

        public string GetInstallLocation() {
            var keyPath = Path.Combine(registryPath, registryName);
            return RegistryWOW6432.TryGetRegKey(registryRoot, keyPath, registryValue);
        }
    }
}
