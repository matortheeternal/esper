using esper.plugins;
using System;

namespace esper.data {
    public struct TES4GroupHeader {
        public Signature signature;
        public UInt32 groupSize;
        public byte[] label;
        public Int32 groupType;
        public byte[] versionControlInfo;
        public UInt32 unknown;

        public TES4GroupHeader(PluginFileSource source) {
            signature = source.ReadSignature();
            groupSize = source.reader.ReadUInt32();
            label = source.reader.ReadBytes(4);
            groupType = source.reader.ReadInt32();
            versionControlInfo = source.reader.ReadBytes(4);
            unknown = source.reader.ReadUInt32();
        }

        internal long WriteTo(PluginFileOutput output) {
            output.writer.Write(signature.bytes);
            output.writer.Write(groupSize);
            output.writer.Write(label);
            output.writer.Write(groupType);
            output.writer.Write(versionControlInfo);
            output.writer.Write(unknown);
            return output.stream.Position;
        }

        internal void WriteUpdatedSize(PluginFileOutput output, long offset) {
            var pos = output.stream.Position;
            UInt32 newSize = (UInt32) (pos - offset);
            if (newSize == groupSize) return;
            output.stream.Position = offset - 20;
            output.writer.Write(newSize);
            output.stream.Position = pos;
        }
    }
}
