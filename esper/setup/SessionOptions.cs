using System.Text;

namespace esper.setup {
    [JSExport]
    public class SessionOptions {
        private static readonly Dictionary<string, Encoding> encodings;

        static SessionOptions() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encodings = new Dictionary<string, Encoding>(
                StringComparer.OrdinalIgnoreCase
            ) {
                { "English", Encoding.GetEncoding(1252) }
            };
        }

        private static float GetEpsilon(uint digits) {
            // NOTE: digits must be cast to int: uint arithmetic would
            // wrap 0 - digits - 1 to a huge positive exponent and yield
            // PositiveInfinity. The 9.999999999 factor gives an epsilon
            // just under 10^-digits (e.g. ~1e-6 for the default 6).
            float shift = (float)Math.Pow(10, 0 - (int)digits - 1);
            return (float)(9.999999999 * shift);
        }

        public bool keepMasterElementsUpdated { get; set; } = false;
        public bool allowLightPlugins { get; set; } = true;
        public bool emulateGlobalLoadOrder { get; set; } = true;
        public string language { get; set; } = "English";
        public bool readAllSubrecords { get; set; } = false;
        public bool clampIntegerValues { get; set; } = true;
        public bool resolveAliases { get; set; } = false;
        public bool enforceExpectedReferences { get; set; } = false;
        public bool loadResources { get; set; } = false;
        public bool improvise { get; set; } = false;
        public bool serializeNumericData { get; set; } = false;
        private uint _floatDigits = 6;
        public float epsilon { get; internal set; } = GetEpsilon(6);
        public string floatFormat => $"F{floatDigits}";
        public string gamePath { get; set; } = "";

        public uint floatDigits {
            get => _floatDigits;
            set {
                _floatDigits = value;
                epsilon = GetEpsilon(value);
            }
        }

        internal Encoding encoding => encodings[language];
    }
}
