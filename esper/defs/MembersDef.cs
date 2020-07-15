using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class MembersDef : Def {
        public MembersDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
        }
    }
}
