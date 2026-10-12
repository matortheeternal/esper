using esper.io;

namespace esper.data.headers {
    //[JSExport]
    public struct TES4GroupHeader : IGroupHeader {
        public static UInt32 size => 24;

        public Signature signature { get; }
        public UInt32 groupSize { get; set; }
        public byte[] label { get; }
        public Int32 groupType { get; }
        public byte[] versionControlInfo { get; }
        public UInt32 unknown { get; }
        private readonly UInt32 headerSize;

        public TES4GroupHeader(PluginFileSource source) {
            headerSize = source.plugin.session.game.groupHeaderSize;
            signature = Signature.Read(source);
            groupSize = source.reader.ReadUInt32();
            label = source.reader.ReadBytes(4);
            groupType = source.reader.ReadInt32();
            versionControlInfo = source.reader.ReadBytes(4);
            unknown = headerSize >= 24 ? source.reader.ReadUInt32() : 0;
        }

        public TES4GroupHeader(
            byte[] label, Int32 groupType, UInt32 headerSize = 24
        ) {
            this.headerSize = headerSize;
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
            if (headerSize >= 24) output.writer.Write(unknown);
            return output.stream.Position;
        }
    }
}
