using esper.defs;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.elements {
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
    }
}
