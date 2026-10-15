using esper.data;
using esper.elements;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class MemberUnionDef : MembersDef {
        public static readonly string defId = "memberUnion";
        public override XEDefType defType => XEDefType.dtSubRecordUnion;
        public override SmashType smashType => SmashType.stUnion;

        public ElementDef defaultDef => memberDefs[0];

        internal MemberUnionDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override Element PrepareElement(Container container) {
            if (defaultDef.IsSubrecord()) return container;
            return new MemberUnionElement(container, this, true);
        }

        public override void SubrecordFound(
            Container container, RecordSource source
        ) {
            int defIndex = 0;
            var subrecord = source.currentSubrecord;
            var memberDef = GetMemberDef(subrecord.signature, ref defIndex);
            if (memberDef == null) return;
            if (memberDef.IsSubrecord()) {
                memberDef.ReadElement(container, source, subrecord.dataSize);
                source.SubrecordHandled();
            } else {
                memberDef.SubrecordFound(container, source);
            }
        }

        public override Element NewElement(Container container = null) {
            var defaultDef = this.defaultDef;
            if (defaultDef.IsSubrecord())
                return defaultDef.NewElement(container);
            return new MemberUnionElement(container, this);
        }

        public override bool CanEnterWith(Signature signature) {
            return ContainsSignature(signature);
        }

        public override bool HasSignature(Signature sig) {
            return defaultDef.IsSubrecord() &&
                memberDefs.Any(d => d.HasSignature(sig));
        }

        public override string GetSortKey(Element element) {
            var elements = ((Container)element)._internalElements;
            if (elements == null || elements.Count == 0) return "";
            return elements[0].sortKey;
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            output.WriteContainer((Container)element);
        }
    }
}
