using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class EnumDef : FormatDef {
        public EnumDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
        }
    }
}
