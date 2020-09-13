using esper.elements;
using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs.TES5 {
    public class HideFFFF_Format : FormatDef {
        public static readonly string defId = "HideFFFF_Format";

        public HideFFFF_Format(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        public override string DataToValue(ValueElement element, dynamic data) {
            if (data == 0xFFFF) return "None";
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            if (value == "None") return 0xFFFF;
            return DataHelpers.ParseInt64(value);
        }
    }
}
