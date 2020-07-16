using esper.defs;
using esper.parsing;
using esper.resolution;

namespace esper.elements {
    public class MainRecord : Container {
        public readonly StructElement header;
        public MainRecordDef mrDef { get => (MainRecordDef) def; }
        public ulong formId {
            get => header.GetData("Form ID");
        }
        public ulong localFormId {
            get => formId & 0xFFFFFF;
        }

        public MainRecord(Container container, Def def) 
            : base(container, def) { }

        public MainRecord(Container container, Def def, PluginFileSource source)
            : base(container, def) {
            header = (StructElement) mrDef.headerDef.ReadElement(this, source);
        }

        public static MainRecord Read(
            PluginFileSource source, 
            Container container, 
            Signature signature
        ) {
            var def = container.manager.GetRecordDef(signature);
            var record = new MainRecord(container, def, source);
            return record;
        }

        public bool IsLocal() {
            // TODO
            return true;
        }
    }
}
