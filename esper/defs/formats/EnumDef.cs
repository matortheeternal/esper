using esper.elements;
using esper.helpers;
using esper.setup;

namespace esper.defs {
    [JSExport]
    public class EnumDef : FormatDef {
        public static Regex unknownOptionExpr = new Regex(@"^<(?:Unknown )?(-?\d+)>$");
        public static readonly string defId = "enum";

        public Dictionary<Int64, string> options;
        public string unknownOption;

        public EnumDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            options = JsonHelpers.Options(src);
            unknownOption = src.Value<string>("unknownOption");
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            var options = this.options;
            Int64 indexKey = data;
            if (!options.ContainsKey(indexKey)) 
                return unknownOption ?? string.Format("<Unknown {0}>", indexKey);
            return options[indexKey];
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            var options = this.options;
            foreach (var (key, option) in options)
                if (option == value) return key;
            var match = unknownOptionExpr.Match(value);
            if (!match.Success) throw new Exception("Invalid option " + value);
            return StringHelpers.DynamicParse(match.Captures[1].Value);
        }
    }
}
