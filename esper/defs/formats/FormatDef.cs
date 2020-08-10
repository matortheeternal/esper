using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class FormatDef : Def {
        public FormatDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public virtual string DataToValue(ValueElement element, dynamic data) {
            throw new NotImplementedException();
        }

        public virtual dynamic ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        public virtual string GetSortKey(ValueElement element, dynamic data) {
            return DataToValue(element, data);
        }
    }
}
