using System.Collections.Generic;
using esper.defs;

namespace esper.elements {
    public class Container : Element {
        internal List<Element> _elements;
        internal MembersDef mdef => (MembersDef) def;

        public virtual List<Element> elements {
            get {
                if (_elements == null) _elements = new List<Element>();
                return _elements;
            }
        }

        public int count => elements.Count;

        public Container(Container container = null, ElementDef def = null) 
            : base(container, def) {}

        public Element FindElementForDef(ElementDef def) {
            for (int i = elements.Count - 1; i >= 0; i--) {
                var element = elements[i];
                if (element.def == def) return element;
            }
            return null;
        }

        internal override void ElementsReady() {
            if (_elements == null) return;
            _elements.ForEach(e => e.ElementsReady());
        }
    }
}
