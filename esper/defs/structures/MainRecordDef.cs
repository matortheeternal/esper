using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class MainRecordDef : MembersDef {
        public static string defType = "record";

        public Def headerDef;

        public MainRecordDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {
            headerDef = manager.BuildMainRecordHeaderDef(src);
        }
    }
}
