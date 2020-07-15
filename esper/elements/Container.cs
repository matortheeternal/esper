using System.Collections.Generic;

namespace esper.elements {
    public class Container : Element {
        public List<Element> elements { get; protected set; }

        public Container(Container container = null, Def def = null) 
            : base(container, def) {
            elements = new List<Element>();
        }
    }
}
