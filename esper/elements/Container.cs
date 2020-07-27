using esper.resolution;
using System.Collections.Generic;
using System.Linq;

namespace esper.elements {
    public class Container : Element {
        public List<Element> elements { get; protected set; }

        public Container(Container container = null, Def def = null) 
            : base(container, def) {
            elements = new List<Element>();
        }

        public Element FindElementForDef(Def def) {
            return elements.Last((element) => element.def == def);
        }
    }
}
