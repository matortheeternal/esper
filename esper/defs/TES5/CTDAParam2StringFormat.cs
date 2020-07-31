using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class CTDAParam2StringFormat : FormatDef {

        public static string defType = "CTDAParam2StringFormat";

        public CTDAParam2StringFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override string DataToValue(ValueElement element, dynamic data) {
            if (element == null) return "";
            var container = element.container;
            if (container == null) return "";
            return container.GetValue(@"..\CIS2");
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
