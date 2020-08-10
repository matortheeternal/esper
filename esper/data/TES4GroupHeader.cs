using esper.plugins;
using System;
using System.IO;

namespace esper.data {
    public class TES4GroupHeader {
        public Signature signature;
        public UInt32 groupSize;
        public byte[] label;
        public Int32 groupType;

        public TES4GroupHeader(PluginFileSource source) {
            signature = source.ReadSignature();
            groupSize = source.reader.ReadUInt32();
            label = source.reader.ReadBytes(4);
            groupType = source.reader.ReadInt32();
            // skip vcs and shit for now
            source.stream.Seek(8, SeekOrigin.Current);
        }
    }
}
