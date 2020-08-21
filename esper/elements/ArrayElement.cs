using esper.defs;
using System.Linq;

namespace esper.elements {
    public class ArrayElement : Container {
        public ArrayDef arrayDef => def as ArrayDef;

        public ArrayElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            var e = arrayDef.elementDef.NewElement(this);
            e.Initialize();
        }

        internal override void ElementsReady() {
            base.ElementsReady();
            if (!arrayDef.sorted || _elements == null) return;
            // we use OrderBy so sortKey is called only once per entry
            _elements = _elements.OrderBy(e => e.sortKey).ToList();
        }
    }
}
