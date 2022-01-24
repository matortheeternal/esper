using esper.plugins;
using System;

namespace esper.data {
    public struct Subrecord {
        public Signature signature;
        public UInt32 dataSize;

        public Subrecord(PluginFileSource source) {
            signature = Signature.Read(source);
            dataSize = source.reader.ReadUInt16();
        }
    }
}
