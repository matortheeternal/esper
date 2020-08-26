using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using esper.defs;

namespace esper.elements {
    public class Container : Element {
        internal List<Element> _internalElements;
        internal MembersDef mdef => (MembersDef)def;

        internal List<Element> internalElements {
            get {
                if (_internalElements == null) 
                    _internalElements = new List<Element>();
                return _internalElements;
            }
        }

        public virtual ReadOnlyCollection<Element> elements {
            get => internalElements.AsReadOnly();
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
            if (_internalElements == null) return;
            _internalElements.ForEach(e => e.ElementsReady());
        }

        internal virtual void ForEachElement(Func<Element, bool> callback) {
            internalElements.ForEach(element => {
                bool stepInto = callback(element);
                if (stepInto && element is Container container)
                    container.ForEachElement(callback);
            });
        }
    }
}
