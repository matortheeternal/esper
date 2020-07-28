using esper.defs;
using esper.parsing;
using esper.resolution;
using System;
using System.Collections.Generic;

namespace esper.elements {
    public class MainRecord : Container, IMainRecord {
        public readonly StructElement header;
        public MainRecordDef mrDef { get => def as MainRecordDef; }
        private long bodyOffset;
        public UInt32 formId => header.GetData("Form ID");
        public UInt32 localFormId => formId & 0xFFFFFF;
        public UInt32 dataSize => header.GetData("Data Size");
        public bool compressed => header.GetFlag("Record Flags", "Compressed");
        public List<Subrecord> unexpectedSubrecords;

        public MainRecord(Container container, Def def) 
            : base(container, def) { }

        public MainRecord(Container container, Def def, PluginFileSource source)
            : base(container, def) {
            header = (StructElement) mrDef.headerDef.ReadElement(this, source);
            bodyOffset = source.stream.Position;
            unexpectedSubrecords = new List<Subrecord>();
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

        private void UnexpectedSubrecord(
            string sig, UInt16 size, PluginFileSource source
        ) {
            var subrecord = new Subrecord(sig, size, source);
            unexpectedSubrecords.Add(subrecord);
        }

        public void ReadElements(PluginFileSource source) {
            source.stream.Position = bodyOffset;
            if (compressed) source.Decompress(dataSize);
            source.ReadMultiple(dataSize, () => {
                var sig = source.ReadSignature().ToString();
                var size = source.reader.ReadUInt16();
                var def = mrDef.GetMemberDef(sig.ToString());
                if (def == null) {
                    UnexpectedSubrecord(sig, size, source);
                } else {
                    def.SubrecordFound(this, source, sig, size);
                }
           });
        }

        public bool IsLocal() {
            // TODO
            return true;
        }
    }
}
