using esper.defs;

namespace esper.elements {
    public class ValueElement : Element {
        public ValueDef valueDef => (ValueDef)def;
        public FormatDef formatDef => valueDef.formatDef;

        public FlagsDef flagsDef {
            get {
                var formatDef = this.formatDef;
                if (formatDef is FormatUnion u)
                    formatDef = u.ResolveDef(container);
                if (formatDef is FlagsDef f) return f;
                return null;
            }
        }

        internal dynamic _data;

        public dynamic data {
            get => _data;
            set => valueDef.SetData(this, value);
        }

        public string value {
            get => valueDef.GetValue(this);
            set => valueDef.SetValue(this, value);
        }

        public static ValueElement Init(
            Container container, ElementDef def, dynamic data
        ) {
            return new ValueElement(container, def, true) {
                _data = data
            };
        }

        public ValueElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            data = valueDef.DefaultData();
        }
    }
}
