using esper.defs;
using esper.parsing;
using esper.resolution;
using System;

namespace esper.elements {
    public class MainRecord : Container, IMainRecord {
        public readonly StructElement header;
        public MainRecordDef mrDef { get => (MainRecordDef) def; }
        private long bodyOffset;
        public ulong formId => header.GetData("Form ID");
        public ulong localFormId => formId & 0xFFFFFF;
        public long dataSize => header.GetData("Data Size");

        public MainRecord(Container container, Def def) 
            : base(container, def) { }

        public MainRecord(Container container, Def def, PluginFileSource source)
            : base(container, def) {
            header = (StructElement) mrDef.headerDef.ReadElement(this, source);
            bodyOffset = source.stream.Position;
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

        public void ReadElements(PluginFileSource source) {
            source.stream.Position = bodyOffset;
            var endOffset = bodyOffset + dataSize;
            while (source.stream.Position < endOffset) {
                break; // TODO
            }
        }

        public bool IsLocal() {
            // TODO
            return true;
        }
    }
}
