using esper.defs;
using esper.parsing;
using esper.resolution;
using System;
using System.Collections.Generic;
using System.IO;

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
            : base(container, def) {}

        public MainRecord(Container container, Def def, PluginFileSource source)
            : base(container, def) {
            header = (StructElement) mrDef.headerDef.ReadElement(this, source);
            bodyOffset = source.stream.Position;
            unexpectedSubrecords = new List<Subrecord>();
            if (sessionOptions.readAllSubrecords) {
                ReadElements(source);
            } else {
                source.stream.Seek(dataSize, SeekOrigin.Current);
            }
        }

        public static MainRecord Read(
            Container container,
            PluginFileSource source,
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
            source.stream.Seek(bodyOffset, SeekOrigin.Begin);
            if (compressed) source.Decompress(dataSize);
            source.ReadMultiple(dataSize, () => {
                var sig = source.ReadSignature().ToString();
                var size = source.reader.ReadUInt16();
                var endPos = source.stream.Position + size;
                var def = mrDef.GetMemberDef(sig.ToString());
                if (def == null) {
                    UnexpectedSubrecord(sig, size, source);
                } else if (def.IsSubrecord()) {
                    def.ReadElement(this, source, size);
                } else {
                    var container = (Container) def.PrepareElement(this);
                    def.SubrecordFound(container, source, sig, size);
                }
                source.stream.Position = endPos;
           });
        }

        public bool IsLocal() {
            // TODO
            return true;
        }
    }
}
