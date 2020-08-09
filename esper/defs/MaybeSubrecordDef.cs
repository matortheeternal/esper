using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class MaybeSubrecordDef : ElementDef {
        public MaybeSubrecordDef(DefinitionManager manager, JObject src, Def parent) 
            : base(manager, src, parent) {
        }

        public override bool ContainsSignature(string signature) {
            return this.signature == signature;
        }

        public override List<string> GetSignatures(List<string> sigs = null) {
            if (sigs == null) sigs = new List<string>();
            if (signature != null) sigs.Add(signature);
            return sigs;
        }
    }
}
