using esper.elements;
using esper.plugins;
using esper.io;
using System;

namespace esper.data.headers {
    public struct TES4RecordHeader : IRecordHeader {
        public static UInt32 size => 24;

        public Signature signature { get; }
        public UInt32 dataSize { get; }
        public UInt32 flags { get; }
        public UInt32 formId { get; }

        public TES4RecordHeader(PluginFileSource source) {
            signature = Signature.Read(source);
            dataSize = source.reader.ReadUInt32();
            flags = source.reader.ReadUInt32();
            formId = source.reader.ReadUInt32();
            source.stream.Position += 8;
        }

        public StructElement ToStructElement(
            MainRecord rec, DataSource source
        ) {
            var headerDef = rec.mrDef.headerDef;
            var structElement = new StructElement(rec, headerDef);
            var defs = headerDef.elementDefs;
            int i = 0;
            ValueElement.Init(structElement, defs[i++], signature.ToString());
            ValueElement.Init(structElement, defs[i++], dataSize);
            ValueElement.Init(structElement, defs[i++], flags);
            var targetFile = rec.file.OrdinalToFile((byte) (formId >> 24), false);
            var fid = new FormId(targetFile, formId & 0xFFFFFF);
            ValueElement.Init(structElement, defs[i++], fid);
            source.stream.Position += 16;
            for (; i < defs.Count; i++)
                defs[i].ReadElement(structElement, source);
            return structElement;
        }
    }
}
