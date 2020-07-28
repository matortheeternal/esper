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
            for (int i = elements.Count - 1; i >= 0; i--) {
                var element = elements[i];
                if (element.def == def) return element;
            }
            return null;
            //return elements.LastOrDefault((element) => element.def == def);
        }
    }
}
