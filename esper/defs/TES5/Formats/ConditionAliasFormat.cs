using esper.elements;
using esper.setup;
using esper.resolution;
using Newtonsoft.Json.Linq;

namespace esper.defs.TES5 {
    public class ConditionAliasFormat : AliasFormat {
        public static string defType = "ConditionAliasFormat";

        public ConditionAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            var rec = element.GetParentElement(e => e is MainRecord);
            return (MainRecord)(rec.signature switch {
                "QUST" => rec,
                "SCEN" => rec.GetElement("@PNAM"),
                "PACK" => rec.GetElement("@QNAM"),
                "INFO" => rec.GetElement(@"..\..\@QNAM"),
                _ => null
            });
        }
    }
}
