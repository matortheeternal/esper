using esper.defs;
using System.Linq;

namespace esper.elements {
    public class ArrayElement : Container {
        public ArrayDef arrayDef => def as ArrayDef;

        public ArrayElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            arrayDef.elementDef.InitElement(this);
        }

        internal override void ElementsReady() {
            base.ElementsReady();
            if (!arrayDef.sorted) return;
            // we use OrderBy so sortKey is called only once per entry
            _elements = _elements.OrderBy(e => e.sortKey).ToList();
        }
    }
}
