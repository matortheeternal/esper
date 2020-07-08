using System;
using System.Runtime.InteropServices;

namespace esper.parsing {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GroupHeader {
        public Signature signature;
        public UInt32 groupSize;
        public Byte4 label;
        public Byte4 groupType;
        public Byte4 versionControlInfo;
        public Byte4 unknown;
    }
}
