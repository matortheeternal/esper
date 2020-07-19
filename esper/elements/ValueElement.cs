using esper.defs;
using esper.parsing;

namespace esper.elements {
    public class ValueElement : Element {
        public dynamic data { get; set; }
        public ValueDef valueDef { get => (ValueDef)def; }
        public FormatDef formatDef { get => valueDef.formatDef; }

        public string value {
            get => valueDef.GetValue(this);
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
