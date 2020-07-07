using esper;
using NUnit.Framework;

namespace Tests {
    public class SignatureEncodingTests {
        SignatureEncoding encoding;

        [OneTimeSetUp]
        public void Setup() {
            encoding = new SignatureEncoding();
        }

        [Test]
        public void TestEncode() {
            byte[] abcd = new byte[4] { 65, 66, 67, 68 };
            byte[] s = new byte[4] { 0, 1, 2, 3 };
            Assert.That(encoding.Encode("ABCD"), Is.EquivalentTo(abcd));
            Assert.That(encoding.Encode("\u0000\u0001\u0002\u0003"), Is.EquivalentTo(s));
        }

        [Test]
        public void TestDecode() {
            byte[] abcd = new byte[4] { 65, 66, 67, 68 };
            byte[] s = new byte[4] { 0, 1, 2, 3 };
            Assert.AreEqual(encoding.Decode(abcd), "ABCD");
            Assert.AreEqual(encoding.Decode(s), "\u0000\u0001\u0002\u0003");
        }
    }
}
