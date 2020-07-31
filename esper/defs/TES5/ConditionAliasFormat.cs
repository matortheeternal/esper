using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class ConditionAliasFormat : FormatDef {

        public static string defType = "ConditionAliasFormat";

        public ConditionAliasFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override string DataToValue(ValueElement element, dynamic data) {
            // TODO: resolve alias
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
