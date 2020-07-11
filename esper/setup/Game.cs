using System;
using System.Linq;

namespace esper {
    public class Game {
        public int xeditId;
        public string name;
        public string baseName;
        public string fullName;
        public string abbreviation;
        public string registryName;
        public string myGamesFolderName;
        public string appDataFolderName;
        public string exeName;
        public string esmName;
        public string iniName;
        public string cccName;
        public string pluginsTxtType;
        public string archiveExtension;
        public string[] pluginExtensions;
        public string[] hardcodedPlugins;
        public int[] steamAppIds;

        public bool SupportsLightPlugins() {
            return pluginExtensions.Contains(".esl");
        }
    }
}
