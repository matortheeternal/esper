using esper.plugins;
using System;

namespace esper.data {
    public struct Subrecord {
        public string signature;
        public UInt32 dataSize;

        public Subrecord(PluginFileSource source) {
            signature = Signature.Read(source).ToString();
            dataSize = source.reader.ReadUInt16();
        }
    }
}
