using esper.io;

namespace esper.data {
    [JSExport]
    public struct Subrecord {
        public Signature signature;
        public UInt32 dataSize;

        public Subrecord(DataSource source) {
            signature = Signature.Read(source);
            dataSize = source.reader.ReadUInt16();
        }
    }
}
