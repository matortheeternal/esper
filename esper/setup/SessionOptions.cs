using System;
using System.Collections.Generic;
using System.Text;

namespace esper.setup {
    public class SessionOptions {
        private static Dictionary<string, Encoding> encodings =
            new Dictionary<string, Encoding> {
                { "English", Encoding.GetEncoding(1252) }
            };

        private static float GetEpsilon(uint digits) {
            float shift = (float)Math.Pow(10, 0 - digits - 1);
            return 9999999999 * shift;
        }

        public bool keepMasterElementsUpdated = false;
        public bool allowLightPlugins = true;
        public bool emulateGlobalLoadOrder = true;
        public string language = "English";
        public bool readAllSubrecords = false;
        public bool clampIntegerValues = true;
        public bool resolveAliases = false;
        public bool enforceExpectedReferences = false;
        public bool loadResources = false;
        public bool serializeNumericData = false;
        private uint _floatDigits = 6;
        public float epsilon = GetEpsilon(6);
        public string floatFormat => $"F{floatDigits}";
        public string gamePath = "";

        public uint floatDigits {
            get => _floatDigits;
            set {
                _floatDigits = value;
                epsilon = GetEpsilon(value);
            }
        }

        public Encoding encoding => encodings[language];
    }
}
