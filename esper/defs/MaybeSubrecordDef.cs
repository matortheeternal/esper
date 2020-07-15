using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class MaybeSubrecordDef : Def {
        public Signature signature;

        public MaybeSubrecordDef(DefinitionManager manager, JObject src, Def parent) 
            : base(manager, src, parent) {
        }

        public void SubrecordFound(Element element, Subrecord subrecord) {
            if (subrecord.signature != signature)
                throw new Exception("Subrecord signature mismatch.");
            DataFound(element, subrecord.GetData());
        }

        public bool HasSignature(Signature signature) {
            return this.signature == signature;
        }

        public bool IsSubrecord() {
            return signature != null;
        }
    }
}
