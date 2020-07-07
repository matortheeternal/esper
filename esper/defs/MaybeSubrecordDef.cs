using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class MaybeSubrecordDef : Def {
        public MaybeSubrecordDef(DefinitionManager manager, JObject src, Def parent = null) 
            : base(manager, src, parent) {

        }
    }
}
