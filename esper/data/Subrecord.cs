using esper.plugins;
using System;

namespace esper.data {
    public struct Subrecord {
        public string signature;
        public UInt16 dataSize;

        public Subrecord(PluginFileSource source) {
            signature = source.ReadSignature().ToString();
            dataSize = source.reader.ReadUInt16();
        }
    }
}
