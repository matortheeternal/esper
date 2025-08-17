using esper.io;

namespace esper.data.headers {
    public struct TES4GroupHeader : IGroupHeader {
        public static UInt32 size => 24;

        public Signature signature { get; }
        public UInt32 groupSize { get; set; }
        public byte[] label { get; }
        public Int32 groupType { get; }
        public byte[] versionControlInfo { get; }
        public UInt32 unknown { get; }

        public TES4GroupHeader(PluginFileSource source) {
            signature = Signature.Read(source);
            groupSize = source.reader.ReadUInt32();
            label = source.reader.ReadBytes(4);
            groupType = source.reader.ReadInt32();
            versionControlInfo = source.reader.ReadBytes(4);
            unknown = source.reader.ReadUInt32();
        }

        public TES4GroupHeader(byte[] label, Int32 groupType) {
            signature = Signature.FromString("GRUP");
            groupSize = 0;
            this.label = label;
            this.groupType = groupType;
            versionControlInfo = new byte[4];
            unknown = 0;
        }

        public long WriteTo(PluginFileOutput output) {
            output.writer.Write(signature.bytes);
            output.writer.Write(groupSize);
            output.writer.Write(label);
            output.writer.Write(groupType);
            output.writer.Write(versionControlInfo);
            output.writer.Write(unknown);
            return output.stream.Position;
        }
    }
}
