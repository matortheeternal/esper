using esper.parsing;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Tests {
    public class Byte2Tests {
        [Test]
        public void TestConstructor() {
            Byte2 twoBytes = new Byte2(0xFF, 0xFF);
            Assert.AreEqual(twoBytes.b0, 0xFF);
            Assert.AreEqual(twoBytes.b1, 0xFF);
        }

        [Test]
        public void TestBytesGetter() {
            Byte2 twoBytes = new Byte2(0xFF, 0xFF);
            Assert.AreEqual(twoBytes.bytes[0], 0xFF);
            Assert.AreEqual(twoBytes.bytes[1], 0xFF);
        }

        [Test]
        public void TestEqualityOperators() {
            Byte2 a = new Byte2(0x12, 0x34);
            Byte2 b = new Byte2(0x12, 0x34);
            Byte2 c = new Byte2(0x56, 0x78);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
        }

        [Test]
        public void TestMarshalling() {
            Byte2 a = new Byte2(0x12, 0x34);
            Assert.AreEqual(Marshal.SizeOf(a), 2);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(a));
            Marshal.StructureToPtr(a, ptr, false);
            Byte2 b = Marshal.PtrToStructure<Byte2>(ptr);
            Assert.IsTrue(a == b);
        }
    }
}
