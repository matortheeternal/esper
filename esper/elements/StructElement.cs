using esper.defs;
using System.Collections.ObjectModel;

namespace esper.elements {
    [JSExport]
    public class StructElement : Container {
        public StructDef structDef => (StructDef) def;

        public override ReadOnlyCollection<Element> elements {
            get {
                if (structDef.elementMap == null) return base.elements;
                return structDef.elementMap.Select(index => {
                    return internalElements[index];
                }).ToList().AsReadOnly();
            }
        }

        public StructElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            structDef.InitChildElements(this);
        }

        internal override AssignmentInfo GetAssignment(ElementDef childDef) {
            AssignmentInfo info = new AssignmentInfo();
            var newOrder = structDef.GetInternalOrder(childDef);
            while (info.index < internalElements.Count) {
                var e = internalElements[index];
                var order = structDef.GetInternalOrder(e.def);
                if (order == newOrder) info.assigned = true;
                if (order >= newOrder) return info;
                info.index++;
            }
            return info;
        }

        internal override bool RemoveElement(Element element) {
            return false;
        }

        internal override Element CopyInto(Container container, CopyOptions options) {
            var element = new StructElement(container, def);
            CopyChildrenInto(element, options);
            return element;
        }
    }
}
