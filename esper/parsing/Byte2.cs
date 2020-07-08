using System.Runtime.InteropServices;

namespace esper.parsing {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Byte2 {
        public readonly byte b0;
        public readonly byte b1;

        public Byte2(byte b0, byte b1) {
            this.b0 = b0;
            this.b1 = b1;
        }

        public byte[] bytes {
            get => new byte[2] { b0, b1 };
        }

        public static bool operator ==(Byte2 a, Byte2 b) =>
            a.b0 == b.b0 &&
            a.b1 == b.b1;

        public static bool operator !=(Byte2 a, Byte2 b) =>
            !(a == b);

        public override bool Equals(object obj) {
            if ((obj == null) || !GetType().Equals(obj.GetType())) return false;
            return this == (Byte2)obj;
        }

        public override int GetHashCode() {
            return b1 << 8 | b0;
        }
    }

}
