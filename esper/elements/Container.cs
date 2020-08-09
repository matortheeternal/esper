using System.Collections.Generic;
using esper.defs;

namespace esper.elements {
    public class Container : Element {
        protected List<Element> _elements;

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
    }
}
