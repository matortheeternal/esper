using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class MainRecordDef : MembersDef {
        public static string defType = "record";
        public StructDef headerDef;

        public MainRecordDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            headerDef = manager.BuildMainRecordHeaderDef(src, this);
        }
    }
}
