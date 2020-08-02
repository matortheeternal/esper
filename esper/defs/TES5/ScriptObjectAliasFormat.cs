using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs.TES5 {
    public class ScriptObjectAliasFormat : AliasFormat {
        public static string defType = "ScriptObjectAliasFormat";

        public ScriptObjectAliasFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return (MainRecord) element?.GetElement(@"..\@FormID");
        }
    }
}
