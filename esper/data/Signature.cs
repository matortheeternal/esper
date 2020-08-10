using System.Runtime.InteropServices;

namespace esper.data {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Signature : Byte4 {
        static readonly SignatureEncoding encoding = new SignatureEncoding();

        public Signature(byte b0, byte b1, byte b2, byte b3) 
            : base(b0, b1, b2, b3) {}

        public Signature(byte[] b)
            : base(b[0], b[1], b[2], b[3]) {}

        public static Signature FromString(string str) {
            byte[] bytes = encoding.Encode(str);
            return new Signature(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public override string ToString() {
            return encoding.Decode(bytes);
        }
    }
}
