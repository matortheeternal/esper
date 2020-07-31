using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class ScriptObjectAliasFormat : FormatDef {
        public static string defType = "ScriptObjectAliasFormat";

        public ScriptObjectAliasFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override string DataToValue(ValueElement element, dynamic data) {
            if (data == -1) return "None";
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            if (value == "None") return -1;
            return Int16.Parse(value);
        }
    }
}
