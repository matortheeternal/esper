using System;

namespace esper.data.headers {
    public interface IGroupHeader {
        public Signature signature { get; }
        public UInt32 groupSize { get; }
        public byte[] label { get; }
        public Int32 groupType { get; }
    }
}
