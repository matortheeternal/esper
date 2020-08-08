using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class PerkDATAQuestStageFormat : FormatDef {
        public static string defType = "PerkDATAQuestStageFormat";

        public PerkDATAQuestStageFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

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
