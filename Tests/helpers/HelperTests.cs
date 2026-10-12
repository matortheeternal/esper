using esper.defs;
using esper.elements;
using esper.helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests.helpers {
    public class DataHelpersTests {
        [Test]
        public void TestParseLeadingUInt() {
            Assert.AreEqual(123u, DataHelpers.ParseLeadingUInt("123"));
            Assert.AreEqual(123u, DataHelpers.ParseLeadingUInt("123abc"));
            Assert.AreEqual(0u, DataHelpers.ParseLeadingUInt("abc"));
            Assert.AreEqual(7u, DataHelpers.ParseLeadingUInt("abc", 7));
            Assert.AreEqual(0u, DataHelpers.ParseLeadingUInt(""));
        }

        [Test]
        public void TestParseInt64() {
            Assert.AreEqual(-42L, DataHelpers.ParseInt64("-42"));
            Assert.AreEqual(0L, DataHelpers.ParseInt64("abc"));
            Assert.AreEqual(99L, DataHelpers.ParseInt64("abc", 99));
        }

        [Test]
        public void TestClampIntegers() {
            Assert.AreEqual(sbyte.MaxValue, DataHelpers.ClampToInt8(1000));
            Assert.AreEqual(sbyte.MinValue, DataHelpers.ClampToInt8(-1000));
            Assert.AreEqual((sbyte)42, DataHelpers.ClampToInt8(42));
            Assert.AreEqual(short.MaxValue, DataHelpers.ClampToInt16(100000));
            Assert.AreEqual(short.MinValue, DataHelpers.ClampToInt16(-100000));
            Assert.AreEqual(int.MaxValue, DataHelpers.ClampToInt32((long)int.MaxValue + 1));
            Assert.AreEqual(int.MinValue, DataHelpers.ClampToInt32((long)int.MinValue - 1));
            Assert.AreEqual(byte.MaxValue, DataHelpers.ClampToUInt8(1000));
            Assert.AreEqual((byte)0, DataHelpers.ClampToUInt8(-5));
            Assert.AreEqual(ushort.MaxValue, DataHelpers.ClampToUInt16(100000));
            Assert.AreEqual(uint.MaxValue, DataHelpers.ClampToUInt32((long)uint.MaxValue + 1));
            Assert.AreEqual(0u, DataHelpers.ClampToUInt32(-1));
        }
    }

    public class StringHelpersTests {
        [Test]
        public void TestFormatBytes() {
            Assert.AreEqual("", StringHelpers.FormatBytes(null));
            Assert.AreEqual("", StringHelpers.FormatBytes(new byte[0]));
            Assert.AreEqual("00", StringHelpers.FormatBytes(new byte[] { 0 }));
            Assert.AreEqual("00 AB FF", StringHelpers.FormatBytes(
                new byte[] { 0x00, 0xAB, 0xFF }
            ));
        }

        [Test]
        public void TestParseBytes() {
            // separators at every third char are skipped
            CollectionAssert.AreEqual(
                new byte[] { 0x00, 0xB0 },
                StringHelpers.ParseBytes("00AB00")
            );
            Assert.Throws<ArgumentException>(
                () => StringHelpers.ParseBytes("00 AB")
            );
            Assert.Throws<Exception>(
                () => StringHelpers.ParseBytes("00ZZ00")
            );
        }

        [Test]
        public void TestFormatParseBytesRoundTripLimitation() {
            // Known limitation: ParseBytes rejects the spaced output
            // produced by FormatBytes (length is never a multiple of 3).
            var formatted = StringHelpers.FormatBytes(new byte[] { 0xDE, 0xAD });
            Assert.AreEqual("DE AD", formatted);
            Assert.Throws<ArgumentException>(
                () => StringHelpers.ParseBytes(formatted)
            );
        }

        [Test]
        public void TestSplitPath() {
            StringHelpers.SplitPath(@"A\B\C", out string head, out string tail);
            Assert.AreEqual("A", head);
            Assert.AreEqual(@"B\C", tail);
            StringHelpers.SplitPath("ABC", out head, out tail);
            Assert.AreEqual("ABC", head);
            Assert.AreEqual("", tail);
        }

        [Test]
        public void TestExtractSignature() {
            Assert.AreEqual("ACTI", StringHelpers.ExtractSignature("ACTI"));
            // labels of the form "XXXX - description" yield null
            Assert.IsNull(StringHelpers.ExtractSignature("ACTI - Activator"));
            Assert.AreEqual("Hell", StringHelpers.ExtractSignature("HelloWorld"));
        }
    }

    public class CollectionHelpersTests {
        private static readonly List<int> list = new List<int> { 1, 3, 5, 7 };

        [Test]
        public void TestBinarySearchFound() {
            var result = CollectionHelpers.BinarySearch(list, e => 5.CompareTo(e));
            Assert.AreEqual(5, result);
        }

        [Test]
        public void TestBinarySearchMissing() {
            var result = CollectionHelpers.BinarySearch(list, e => 4.CompareTo(e));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestBinarySearchReturnLast() {
            var result = CollectionHelpers.BinarySearch(
                list, e => 99.CompareTo(e), true
            );
            Assert.AreEqual(7, result);
        }
    }

    public class JsonHelpersTests {
        [Test]
        public void TestObjectAssign() {
            var target = new JObject { { "a", 1 } };
            var result = JsonHelpers.ObjectAssign(
                target, new JObject { { "b", 2 } }
            );
            Assert.AreEqual(1, result.Value<int>("a"));
            Assert.AreEqual(2, result.Value<int>("b"));
        }

        [Test]
        public void TestList() {
            var src = new JObject { { "items", new JArray { "x", "y" } } };
            CollectionAssert.AreEqual(
                new List<string> { "x", "y" },
                JsonHelpers.List<string>(src, "items")
            );
            Assert.IsNull(JsonHelpers.List<string>(src, "missing"));
        }

        [Test]
        public void TestParseOptionKey() {
            Assert.AreEqual(123L, JsonHelpers.ParseOptionKey("123"));
            // 4-char keys become little-endian uint32s of their ASCII bytes
            Assert.AreEqual(0x44434241L, JsonHelpers.ParseOptionKey("ABCD"));
        }

        [Test]
        public void TestOptions() {
            var src = new JObject {
                { "options", new JObject { { "1", "One" } } }
            };
            var options = JsonHelpers.Options(src);
            Assert.AreEqual("One", options[1]);
            Assert.IsNull(JsonHelpers.Options(new JObject()));
        }

        [Test]
        public void TestFlags() {
            var src = new JObject {
                { "flags", new JObject { { "3", "Third" } } }
            };
            var flags = JsonHelpers.Flags(src, "flags");
            Assert.AreEqual("Third", flags[3]);
            Assert.IsNull(JsonHelpers.Flags(new JObject(), "flags"));
        }

        [Test]
        public void TestDictionary() {
            var src = new JObject {
                { "map", new JObject { { "k", "v" } } }
            };
            Assert.AreEqual("v", JsonHelpers.Dictionary(src, "map")["k"]);
            Assert.IsNull(JsonHelpers.Dictionary(src, "missing"));
        }

        [Test]
        public void TestParseConflictType() {
            Assert.AreEqual(
                ConflictType.Normal,
                JsonHelpers.ParseConflictType(new JObject())
            );
            Assert.AreEqual(
                ConflictType.Benign,
                JsonHelpers.ParseConflictType(
                    new JObject { { "conflictType", "Benign" } }
                )
            );
        }
    }

    public class ErrorHelpersTests {
        [Test]
        public void TestCheckDefPropertyPasses() {
            var src = new JObject { { "type", "uint8" } };
            Assert.DoesNotThrow(() => ErrorHelpers.CheckDefProperty(src, "type"));
        }

        [Test]
        public void TestCheckDefPropertyThrows() {
            var ex = Assert.Throws<Exception>(
                () => ErrorHelpers.CheckDefProperty(new JObject(), "type")
            );
            StringAssert.Contains("type", ex.Message);
            StringAssert.Contains("Expected def property", ex.Message);
        }
    }

    public class ElementHelpersTests {
        [Test]
        public void TestStructSortKeyNull() {
            Assert.AreEqual("", ElementHelpers.StructSortKey(null, null));
        }
    }
}
