using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class MaybeSubrecordDef : ElementDef {
        public MaybeSubrecordDef(DefinitionManager manager, JObject src, Def parent) 
            : base(manager, src, parent) {
        }

        public override bool ContainsSignature(string signature) {
            return this.signature == signature;
        }
    }
}
