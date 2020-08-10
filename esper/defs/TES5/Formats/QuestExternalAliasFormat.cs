using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs.TES5 {
    public class QuestExternalAliasFormat : AliasFormat {
        public static string defType = "QuestExternalAliasFormat";

        public QuestExternalAliasFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {}

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return (MainRecord)element.GetElement(@"..\@ALEQ");
        }
    }
}
