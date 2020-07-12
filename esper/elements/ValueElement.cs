using esper.data;
using esper.defs;

namespace esper.elements {
    public class ValueElement : Element {
        public DataContainer data { get; set; }

        public string value {
            get {
                return data.ToString();
            }
            set {
                var vdef = (ValueDef)def;
                vdef.SetValue(this, value);
            }
        }

        public ValueElement(Container container, Def src)
            : base(container, src) { 
        }
    }
}
