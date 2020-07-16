using esper.parsing;
using System;
using System.Buffers.Binary;

namespace esper.helpers {
    public static class BinaryHelpers {
        public static UInt32 ReadUInt32(ReadOnlySpan<byte> bytes, int offset) {
            return BinaryPrimitives.ReadUInt32LittleEndian(
                bytes.Slice(offset, 4)
            );
        }

        public static Signature ReadSignature(ReadOnlySpan<byte> bytes, int offset) {
            return new Signature(
                bytes[offset], 
                bytes[offset + 1], 
                bytes[offset + 2], 
                bytes[offset + 3]
            );
        }

        public static Byte4 ReadByte4(ReadOnlySpan<byte> bytes, int offset) {
            return new Byte4(
                bytes[offset],
                bytes[offset + 1],
                bytes[offset + 2],
                bytes[offset + 3]
            );
        }
    }
}
