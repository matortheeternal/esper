using System;
using System.IO;

namespace esper.parsing {
    public class TES4RecordHeader {
        public Signature signature;
        public UInt32 dataSize;
        public UInt32 flags;
        public UInt32 formId;

        public TES4RecordHeader(PluginFileSource source) {
            signature = source.ReadSignature();
            dataSize = source.reader.ReadUInt32();
            flags = source.reader.ReadUInt32();
            formId = source.reader.ReadUInt32();
            // skip vcs and shit for now
            source.stream.Seek(8, SeekOrigin.Current); 
        }
    }
}
