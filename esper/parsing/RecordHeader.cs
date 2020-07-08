using System;
using System.Runtime.InteropServices;

namespace esper.parsing {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RecordHeader {
        public Signature signature;
        public UInt32 dataSize;
        public UInt32 flags;
        public UInt32 formId;
        public Byte4 versionControl1;
        public Byte2 formVersion;
        public Byte2 versionControl2;
    }
}
