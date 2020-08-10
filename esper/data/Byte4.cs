using System.Runtime.InteropServices;

namespace esper.data {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Byte4 {
        public readonly byte b0;
        public readonly byte b1;
        public readonly byte b2;
        public readonly byte b3;

        public Byte4(byte b0, byte b1, byte b2, byte b3) {
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
        }

        public byte[] bytes {
            get => new byte[4] { b0, b1, b2, b3 };
        }

        public static bool operator ==(Byte4 a, Byte4 b) =>
            a.b0 == b.b0 &&
            a.b1 == b.b1 &&
            a.b2 == b.b2 &&
            a.b3 == b.b3;

        public static bool operator !=(Byte4 a, Byte4 b) =>
            !(a == b);

        public override bool Equals(object obj) {
            if ((obj == null) || !GetType().Equals(obj.GetType())) return false;
            return this == (Byte4)obj;
        }

        public override int GetHashCode() {
            return b3 << 24 | b2 << 16 | b1 << 8 | b0;
        }
    }

}
