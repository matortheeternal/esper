using esper.parsing;
using NUnit.Framework;
using System;

namespace Tests {
    public class SpanTest {
        [Test]
        public void ClassSpan() {
            var header = new ReadOnlySpan<RecordHeader>(new RecordHeader[] {
                new RecordHeader {
                    signature = Signature.FromString("ABCD"),
                    dataSize = 50,
                    flags = 1 << 3 | 1 << 7 | 1 << 8,
                    formId = 0x00123456,
                    versionControl1 = new Byte4(0x1, 0x2, 0x3, 0x4),
                    formVersion = new Byte2(0, 43),
                    versionControl2 = new Byte2(0, 0)
                }
            });
            Assert.AreEqual(header[0].signature.ToString(), "ABCD");
            Assert.AreEqual(header[0].dataSize, 50);
            Assert.AreEqual(header[0].formId, 0x123456);
        }
    }
}
