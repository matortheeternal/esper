using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class EnumDef : FormatDef {
        public EnumDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {
        }
    }
}
