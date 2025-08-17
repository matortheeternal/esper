using esper.data;
using esper.elements;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class MaybeSubrecordDef : ElementDef {
        internal readonly Signature _signature;

        public override Signature signature => _signature;
        public override string displayName => _signature != Signatures.None
            ? $"{_signature} - {name}"
            : name;

        public MaybeSubrecordDef(DefinitionManager manager, JObject src) 
            : base(manager, src) {
            var sig = src.Value<string>("signature");
            _signature = Signature.FromString(sig);
        }

        public MaybeSubrecordDef(MaybeSubrecordDef other) : base(other) {
            _signature = other._signature;
        }

        public override bool ContainsSignature(Signature signature) {
            return this.signature == signature;
        }

        public override bool CanEnterWith(Signature signature) {
            return this.signature == signature;
        }

        public override HashSet<Signature> GetSignatures(HashSet<Signature> sigs = null) {
            if (sigs == null) sigs = new HashSet<Signature>();
            if (signature != null) sigs.Add(signature);
            return sigs;
        }

        internal override UInt32 GetSize(Element element) {
            return (UInt32) ((IsSubrecord() ? 6 : 0) + base.GetSize(element));
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            if (!IsSubrecord()) return;
            output.WriteSignature(_signature);
            output.writer.Write((UInt16) (element.def.GetSize(element) - 6));
        }
    }
}
