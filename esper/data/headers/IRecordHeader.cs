using System;
using Microsoft.JavaScript.NodeApi;

namespace esper.data.headers {
    [JSExport]
    public interface IRecordHeader {
        public Signature signature { get; }
        public UInt32 dataSize { get; }
        public UInt32 flags { get; }
        public UInt32 formId { get; }
    }
}
