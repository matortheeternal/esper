using esper.plugins;

namespace esper.data {
    public struct Signature {
        static readonly SignatureEncoding encoding = new SignatureEncoding();

        readonly byte b0;
        readonly byte b1;
        readonly byte b2;
        readonly byte b3;

        public byte[] bytes {
            get => new byte[4] { b0, b1, b2, b3 };
        }

        public Signature(byte b0, byte b1, byte b2, byte b3) {
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
        }

        public Signature(byte[] b) {
            b0 = b[0];
            b1 = b[1];
            b2 = b[2];
            b3 = b[3];
        }

        public static Signature FromString(string str) {
            byte[] bytes = encoding.Encode(str);
            return new Signature(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public override string ToString() {
            return encoding.Decode(bytes);
        }

        public static bool operator ==(Signature a, Signature b) =>
            a.b0 == b.b0 &&
            a.b1 == b.b1 &&
            a.b2 == b.b2 &&
            a.b3 == b.b3;

        public static bool operator !=(Signature a, Signature b) =>
            !(a == b);

        public override bool Equals(object obj) {
            if ((obj == null) || !GetType().Equals(obj.GetType())) return false;
            return this == (Signature)obj;
        }

        public override int GetHashCode() {
            return b3 << 24 | b2 << 16 | b1 << 8 | b0;
        }

        internal static Signature Read(PluginFileSource source) {
            return new Signature(source.reader.ReadBytes(4));
        }
    }
}
