using System;
using System.Linq;

namespace esper {
    public unsafe struct Signature {
        static SignatureEncoding encoding = new SignatureEncoding();

        private fixed byte data[4];

        public Signature(byte[] data) {
            this.data = data;
        }

        public Signature(string str) {
            data = encoding.Encode(str);
        }

        public static bool operator ==(Signature a, Signature b) =>
            a.data.SequenceEqual(b.data);

        public static bool operator !=(Signature a, Signature b) =>
            !(a.data == b.data);

        public override bool Equals(object obj) {
            if ((obj == null) || !GetType().Equals(obj.GetType())) return false;
            return this == (Signature)obj;
        }
        public override int GetHashCode() {
            return BitConverter.ToInt32(data, 0);
        }

        public override string ToString() {
            return encoding.Decode(data);
        }
    }
}
