using esper.defs;

namespace esper.elements {
    public class ValueElement : Element {
        public ValueDef valueDef => (ValueDef)def;
        public FormatDef formatDef => valueDef.formatDef;

        internal dynamic _data;

        public dynamic data {
            get => _data;
            set => valueDef.SetData(this, value);
        }

        public string value {
            get => valueDef.GetValue(this);
            set => valueDef.SetValue(this, value);
        }

        public ValueElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            data = valueDef.DefaultData();
        }
    }
}
