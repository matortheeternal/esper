using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class CTDAParam1StringFormat : FormatDef {

        public static string defType = "CTDAParam1StringFormat";

        public CTDAParam1StringFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override string DataToValue(ValueElement element, dynamic data) {
            if (element == null) return "";
            var container = element.container;
            if (container == null) return "";
            return element.GetValue(@"..\CIS1");
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
