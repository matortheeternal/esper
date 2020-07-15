using esper.data;
using esper.defs;
using esper.parsing;

namespace esper.elements {
    public class ValueElement : Element {
        public DataContainer data { get; set; }
        public ValueDef valueDef { get => (ValueDef)def; }

        public string value {
            get => data.ToString();
            set => valueDef.SetValue(this, value);
        }

        public ValueElement(Container container, Def def)
            : base(container, def) {
            data = valueDef.DefaultData();
        }

        public ValueElement(Container container, Def def, PluginFileSource source)
            : base(container, def) {
            data = valueDef.ReadData(source);
        }
    }
}
