using System;

namespace esper {
    public class SignatureEncoding {
        public SignatureEncoding() {}

        public byte[] Encode(string str) {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                char c = str[i];
                bytes[i] = Convert.ToByte(c);
            }
            return bytes;
        }

        public string Decode(byte[] data) {
            char[] chars = new char[4];
            for (int i = 0; i < 4; i++) {
                byte b = data[i];
                chars[i] = Convert.ToChar(b);
            }
            return new string(chars);
        }
    }
}