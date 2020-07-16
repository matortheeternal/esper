using esper.data;
using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class FormatDef : Def {
        public FormatDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public string DataToValue(ValueElement element, dynamic data) {
            throw new NotImplementedException();
        }

        public dynamic ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }
    }
}
