using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs.TES5 {
    public class PackageLocationAliasFormat : AliasFormat {
        public static string defType = "PackageLocationAliasFormat";

        public PackageLocationAliasFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {}

        public override MainRecord ResolveQuestRec(ValueElement element) {
            var rec = element.GetParentElement(e => e is MainRecord);
            return (MainRecord) rec?.GetElement("@QNAM");
        }
    }
}
