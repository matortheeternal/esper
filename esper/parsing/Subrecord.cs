using System;
using System.IO;

namespace esper.parsing {
    public class Subrecord {
        public Signature signature;
        public UInt16 size;
        public byte[] data;

        public Subrecord(string signature, UInt16 size, PluginFileSource source) {
            this.signature = Signature.FromString(signature);
            this.size = size;
            source.stream.Seek(size, SeekOrigin.Current);
        }
    }
}
