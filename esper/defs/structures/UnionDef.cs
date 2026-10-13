using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class UnionDef : MaybeSubrecordDef {
        public static readonly string defId = "union";
        public override XEDefType defType => XEDefType.dtUnion;
        public override SmashType smashType => SmashType.stUnion;

        private readonly bool _canContainFormIds;
        private readonly Decider decider;
        public List<ElementDef> elementDefs;

        public override bool canContainFormIds => _canContainFormIds;
        public override List<ElementDef> childDefs => elementDefs;

        internal UnionDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            elementDefs = JsonHelpers.Defs<ElementDef>(manager, src, "elements");
            decider = JsonHelpers.Decider(manager, src);
            _canContainFormIds = elementDefs.Any(d => d.canContainFormIds);
        }

        public ElementDef ResolveDef(Container container) {
            return ResolveDef(container, null);
        }

        public ElementDef ResolveDef(Container container, UInt32? dataSize) {
            var index = decider.Decide(container, dataSize);
            return elementDefs[index];
        }

        public override Element ReadElement(
            Container container, DataSource source, UInt32? dataSize = null
        ) {
            var resolvedDef = ResolveDef(container, dataSize);
            if (!IsSubrecord() && resolvedDef is ValueDef valueDef) {
                return new UnionValueElement(container, valueDef, this) {
                    _data = valueDef.ReadData(source, dataSize)
                };
            } else {
                var e = new UnionElement(container, this);
                resolvedDef.ReadElement(e, source, dataSize);
                return e;
            }
        }

        public override Element NewElement(Container container = null) {
            var resolvedDef = ResolveDef(container, null);
            if (resolvedDef is ValueDef)
                return new UnionValueElement(container, resolvedDef, this);
            return new UnionElement(container, this);
        }

        public override string GetSortKey(Element element) {
            var elements = ((Container)element)._internalElements;
            if (elements == null || elements.Count == 0) return "";
            return elements[0].sortKey;
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            base.WriteElement(element, output);
            output.WriteContainer((Container)element);
        }
    }
}
