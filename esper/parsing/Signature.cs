using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace esper {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Signature {
        static SignatureEncoding encoding = new SignatureEncoding();

        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte b0;

        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte b1;

        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte b2;

        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte b3;

        public byte[] bytes {
            get {
                byte[] b = new byte[4];
                b[0] = b0;
                b[1] = b1;
                b[2] = b2;
                b[3] = b3;
                return b;
            }
            set {
                b0 = value[0];
                b1 = value[1];
                b2 = value[2];
                b3 = value[3];
            }
        }

        public Signature(string str) {
            bytes = encoding.Encode(str);
        }

        public static bool operator ==(Signature a, Signature b) =>
            a.bytes.SequenceEqual(b.bytes);

        public static bool operator !=(Signature a, Signature b) =>
            !(a == b);

        public override bool Equals(object obj) {
            if ((obj == null) || !GetType().Equals(obj.GetType())) return false;
            return this == (Signature)obj;
        }
        public override int GetHashCode() {
            return Convert.ToInt32(bytes);
        }

        public override string ToString() {
            return encoding.Decode(bytes);
        }
    }
}
