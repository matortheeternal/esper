using esper.elements;
using esper.plugins;
using System;
using System.IO;

namespace esper.data {
    public struct TES4RecordHeader {
        public Signature signature;
        public UInt32 dataSize;
        public UInt32 flags;
        public UInt32 formId;

        private string sig => signature.ToString();

        internal TES4RecordHeader(PluginFileSource source) {
            signature = source.ReadSignature();
            dataSize = source.reader.ReadUInt32();
            flags = source.reader.ReadUInt32();
            formId = source.reader.ReadUInt32();
            // TODO: get this offset from the definition manager?
            source.stream.Seek(8, SeekOrigin.Current); 
        }

        internal StructElement ToStructElement(
            MainRecord rec, PluginFileSource source
        ) {
            var headerDef = rec.mrDef.headerDef;
            var structElement = new StructElement(rec, headerDef);
            var defs = headerDef.elementDefs;
            int i = 0;
            ValueElement.Init(structElement, defs[i++], sig);
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

        internal void WriteUpdatedSize(PluginFileOutput output, long offset) {
            var pos = output.stream.Position;
            UInt32 newSize = (UInt32) (pos - offset);
            if (newSize == dataSize) return;
            output.stream.Position = offset - 20;
            output.writer.Write(newSize);
            output.stream.Position = pos;
        }
    }
}
