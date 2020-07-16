using System.Collections.Generic;
using System.Text;

namespace esper.setup {
    public class SessionOptions {
        private static Dictionary<string, Encoding> encodings =
            new Dictionary<string, Encoding> {
                { "English", Encoding.GetEncoding("windows-1251") }
            };

        public bool keepMasterElementsUpdated = false;
        public bool allowLightPlugins = true;
        public bool emulateGlobalLoadOrder = true;
        public string language = "English";
        public bool buildDefsOnDemand = false;

        public Encoding encoding { get => encodings[language]; }
    }
}
