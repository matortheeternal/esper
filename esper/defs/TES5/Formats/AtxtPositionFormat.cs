using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class AtxtPositionFormat : FormatDef {
        public static string defType = "AtxtPositionFormat";

        public AtxtPositionFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        public override string DataToValue(ValueElement element, dynamic data) {
            Int64 v = data;
            return $"{v} -> {v / 17}:{v % 17}";
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }
    }
}
