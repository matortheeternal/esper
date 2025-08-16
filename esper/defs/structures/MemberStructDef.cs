using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;
using esper.data;

namespace esper.defs {
    [JSExport]
    public class MemberStructDef : MembersDef {
        public static readonly string defId = "memberStruct";
        public override XEDefType defType => XEDefType.dtStruct;
        public override SmashType smashType => SmashType.stStruct;

        private readonly List<int> sortKeyIndices;
        private readonly bool unordered;

        public MemberStructDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            sortKeyIndices = JsonHelpers.List<int>(src, "sortKey");
            unordered = src.Value<bool>("unordered");
        }

        public override Element PrepareElement(Container container) {
            return new MemberStructElement(container, this);
        }

        internal bool HandleSubrecord(
            Container container, RecordSource source, ref int defIndex
        ) {
            var subrecord = source.currentSubrecord;
            var def = GetMemberDef(subrecord.signature, ref defIndex);
            if (def == null) return false;
            if (def.IsSubrecord()) {
                def.ReadElement(container, source, subrecord.dataSize);
                source.SubrecordHandled();
            } else {
                var e = (Container)def.PrepareElement(container);
                def.SubrecordFound(e, source);
            }
            defIndex++;
            return true;
        }

        public override bool CanEnterWith(Signature signature) {
            if (unordered) return ContainsSignature(signature);
            return memberDefs[0].CanEnterWith(signature);
        }

        public override void SubrecordFound(
            Container container, RecordSource source
        ) {
            int defIndex = 0;
            while (defIndex < memberDefs.Count) {
                bool handled = HandleSubrecord(container, source, ref defIndex);
                if (!handled) break;
                if (!source.NextSubrecord()) break;
            }
        }

        public void InitChildElements(Container container) {
            foreach (var memberDef in memberDefs) {
                if (!memberDef.required) continue;
                var e = memberDef.NewElement(container);
                e.Initialize();
            }
        }

        public override string GetSortKey(Element element) {
            return ElementHelpers.StructSortKey(element, sortKeyIndices);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            output.WriteContainer((Container)element);
        }

        internal int GetInternalOrder(ElementDef childDef) {
            var index = memberDefs.IndexOf(childDef);
            if (index == -1)
                throw new Exception($"Element {childDef.name} is not supported.");
            return index;
        }
    }
}
