using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class HideFFFFFormat : FormatDef {
        public static string defType = "HideFFFFFormat";

        public HideFFFFFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override string DataToValue(ValueElement element, dynamic data) {
            if (data == 0xFFFF) return "None";
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }
    }
}
