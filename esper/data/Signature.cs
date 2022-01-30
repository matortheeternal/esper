using esper.plugins;
using System;

namespace esper.data {
    public struct Signature {
        static readonly SignatureEncoding encoding = new SignatureEncoding();

        internal readonly int v;

        public byte[] bytes => BitConverter.GetBytes(v);

        public Signature(int v) {
            this.v = v;
        }

        public Signature(byte[] bytes) {
            v = BitConverter.ToInt32(bytes);
        }

        public static Signature FromString(string str) {
            return str == null 
                ? new Signature(0)
                : new Signature(encoding.Encode(str));
        }

        public override string ToString() {
            return encoding.Decode(bytes);
        }

        public static bool operator ==(Signature a, Signature b) =>
            a.v == b.v;

        public static bool operator !=(Signature a, Signature b) =>
            !(a == b);

        public override bool Equals(object obj) {
            if ((obj == null) || !GetType().Equals(obj.GetType())) return false;
            return this == (Signature)obj;
        }

        public override int GetHashCode() {
            return v;
        }

        internal static Signature Read(PluginFileSource source) {
            return new Signature(source.reader.ReadInt32());
        }
    }
}
