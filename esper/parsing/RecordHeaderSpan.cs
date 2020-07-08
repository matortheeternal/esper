using esper.helpers;
using System;
using System.Buffers.Binary;

namespace esper.parsing {
    public ref struct RecordHeaderSpan {
        public static int size = 24;

        readonly ReadOnlySpan<byte> bytes;

        public Signature signature => BinaryHelpers.ReadSignature(bytes, 0);
        public UInt32 dataSize => BinaryHelpers.ReadUInt32(bytes, 4);
        public UInt32 flags => BinaryHelpers.ReadUInt32(bytes, 8);
        public UInt32 formId => BinaryHelpers.ReadUInt32(bytes, 12);
        public Byte4 versionControl1 => BinaryHelpers.ReadByte4(bytes, 16);
        public Byte2 formVersion => BinaryHelpers.ReadByte2(bytes, 20);
        public Byte2 versionControl2 => BinaryHelpers.ReadByte2(bytes, 22);

        public RecordHeaderSpan(ReadOnlySpan<byte> buffer) {
            bytes = buffer.Slice(0, RecordHeaderSpan.size);
        }
    }
}
