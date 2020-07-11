using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class FlagsDef : FormatDef {
        public FlagsDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {
        }
    }
}
