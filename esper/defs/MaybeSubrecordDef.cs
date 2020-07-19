using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class MaybeSubrecordDef : Def {
        public MaybeSubrecordDef(DefinitionManager manager, JObject src, Def parent) 
            : base(manager, src, parent) {
        }

        public override void SubrecordFound(Element element, Subrecord subrecord) {
            if (subrecord.signature.ToString() != signature)
                throw new Exception("Subrecord signature mismatch.");
            //DataFound(element, subrecord.GetData());
        }

        public override bool ContainsSignature(string signature) {
            return this.signature == signature;
        }

        public bool IsSubrecord() {
            return signature != null;
        }
    }
}
