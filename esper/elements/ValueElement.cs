using esper.defs;

namespace esper.elements {
    public class ValueElement : Element {
        public dynamic data { get; set; }
        public ValueDef valueDef { get => (ValueDef)def; }
        public FormatDef formatDef { get => valueDef.formatDef; }

        public string value {
            get => valueDef.GetValue(this);
            set => valueDef.SetValue(this, value);
        }

        public ValueElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            data = valueDef.DefaultData();
        }
    }
}
