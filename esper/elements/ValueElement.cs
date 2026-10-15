using esper.data;
using esper.defs;
using esper.io;

namespace esper.elements {
    [JSExport]
    public class ValueElement : Element {
        public ValueDef valueDef => (ValueDef)def;
        public FormatDef formatDef => valueDef.formatDef;

        public FlagsDef flagsDef {
            get {
                var formatDef = this.formatDef;
                if (formatDef is FormatUnion u)
                    formatDef = u.ResolveDef(container as Container);
                if (formatDef is FlagsDef f) return f;
                return null;
            }
        }

        internal VariantData _data;

        internal VariantData data {
            get => _data;
            set => valueDef.SetData(this, value);
        }

        public string value {
            get => valueDef.GetValue(this);
            set => valueDef.SetValue(this, value);
        }

        public override UInt32 size => def.GetSize(this);

        public override MainRecord referencedRecord {
            get {
                if (valueDef is FormIdDef) {
                    return data is VariantData.Form form
                        ? form.V.ResolveRecord()
                        : null;
                }
                return base.referencedRecord;
            }
            set {
                data = FormId.FromSource(value.file, value.fileFormId);
            }
        }

        internal static ValueElement Init(
            Container container, ElementDef def, VariantData data
        ) {
            return new ValueElement(container, def) {
                _data = data
            };
        }

        public ValueElement(Container container, ElementDef def)
            : base(container, def) { }

        public override void Initialize() {
            data = valueDef.DefaultData();
        }

        internal override void WriteTo(PluginFileOutput output) {
            valueDef.WriteElement(this, output);
        }

        internal override Element CopyInto(Element container, CopyOptions options) {
            return ValueElement.Init(container as Container, def, _data);
        }

        public override void BuildRefBy() {
            var rec = (valueDef is FormIdDef) ? referencedRecord : null;
            if (rec != null) rec.AddRef(record);
        }

        internal override JToken ToJson() {
            if (sessionOptions.serializeNumericData && valueDef.isNumeric)
                return data?.ToJToken();
            return value;
        }
    }
}
