using esper.defs;

namespace esper.elements {
    public class ArrayElement : Container {
        public ArrayDef arrayDef => def as ArrayDef;

        public ArrayElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            arrayDef.elementDef.InitElement(this);
        }
    }
}
