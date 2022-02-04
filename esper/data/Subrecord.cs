using esper.io;
using System;

namespace esper.data {
    public struct Subrecord {
        public Signature signature;
        public UInt32 dataSize;

        public Subrecord(DataSource source) {
            signature = Signature.Read(source);
            dataSize = source.reader.ReadUInt16();
        }
    }
}
