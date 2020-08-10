using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs.TES5 {
    public class QuestAliasFormat : AliasFormat {
        public static string defType = "QuestAliasFormat";

        public QuestAliasFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {}

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return (MainRecord) element.GetParentElement(e => e is MainRecord);
        }
    }
}
