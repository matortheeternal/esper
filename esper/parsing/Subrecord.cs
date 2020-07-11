using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace esper.parsing {
    public class Subrecord {
        public Signature signature;
        public ushort size;
        public IntPtr dataPtr;

        public Subrecord(Signature signature, ushort size, IntPtr dataPtr) {
            this.signature = signature;
            this.size = size;
            this.dataPtr = dataPtr;
        }
        
        public static Subrecord Read(MemoryMappedViewStream stream) {
            unsafe {
                void* ptr = (void*)stream.PositionPointer;
                Span<byte> span = new Span<byte>(ptr, 6);
                var signature = new Signature(span[0], span[1], span[2], span[3]);
                var size = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(4, 2));
                IntPtr dataPtr = ((IntPtr)stream.PositionPointer) + 6;
                stream.Seek(stream.PointerOffset + 6 + size, SeekOrigin.Begin);
                return new Subrecord(signature, size, dataPtr);
            }
        }
    }
}
