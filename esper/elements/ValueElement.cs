using esper.data;
using esper.defs;
using esper.plugins;
using System;

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

        public override UInt32 size => def.GetSize(this);

        public override MainRecord referencedRecord {
            get {
                if (valueDef is FormIdDef) {
                    return data is FormId fid
                        ? fid.ResolveRecord() 
                        : null;
                }
                return base.referencedRecord;
            }
            set {
                data = FormId.FromSource(value.file, value.formId);
            }
        }

        public static ValueElement Init(
            Container container, ElementDef def, dynamic data
        ) {
            return new ValueElement(container, def) {
                _data = data
            };
        }

        public ValueElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            data = valueDef.DefaultData();
        }

        internal override void WriteTo(PluginFileOutput output) {
            valueDef.WriteElement(this, output);
        }
    }
}
