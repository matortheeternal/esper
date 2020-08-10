using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class _180DivPiFormat : FormatDef {
        public static string defType = "180DivPiFormat";

        public _180DivPiFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string DataToValue(ValueElement element, dynamic data) {
            // TODO
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
