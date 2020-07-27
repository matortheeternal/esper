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

        public override void ReadSubrecord(
            Container container, PluginFileSource source, Signature signature, UInt16 size
        ) {
            if (this.signature != signature.ToString())
                throw new Exception("Def signature mismatch.");
            ReadElement(container, source, size);
        }

        public override bool ContainsSignature(string signature) {
            return this.signature == signature;
        }

        public bool IsSubrecord() {
            return signature != null;
        }
    }
}
