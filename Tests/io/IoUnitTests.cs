using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.io;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests.io {
    public class StructApiTests {
        [Test]
        public void TestSignatureRoundTrip() {
            var sig = Signature.FromString("ACTI");
            Assert.AreEqual("ACTI", sig.ToString());
            Assert.AreEqual(sig, Signature.FromString("ACTI"));
            Assert.IsTrue(sig == Signature.FromString("ACTI"));
            Assert.IsFalse(sig != Signature.FromString("ACTI"));
            Assert.AreEqual(sig.GetHashCode(),
                Signature.FromString("ACTI").GetHashCode());
            Assert.IsTrue(sig.Equals((object)Signature.FromString("ACTI")));
            // little-endian byte order: 'A' first
            Assert.AreEqual((byte)'A', sig.bytes[0]);
            Assert.AreEqual((byte)'I', sig.bytes[3]);
        }

        [Test]
        public void TestSubrecordStruct() {
            var sub = new Subrecord {
                signature = Signature.FromString("EDID"),
                dataSize = 6
            };
            Assert.AreEqual("EDID", sub.signature.ToString());
            Assert.AreEqual(6, sub.dataSize);
        }

        [Test]
        public void TestFormIdMath() {
            var formId = new FormId(null, 0xF);
            Assert.AreEqual(0xFu, formId.fileFormId);
            Assert.AreEqual(0xFu, formId.localFormId);
            // low local ids resolve to the hardcoded master slot
            Assert.AreEqual("Hardcoded", formId.targetFileName);
        }

        [Test]
        public void TestFormIdResolveRecordNullSafe() {
            var formId = new FormId(null, 0x1234);
            Assert.IsNull(formId.ResolveRecord());
        }
    }

    public class RecordSourceTests {
        private Session session;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
        }

        private MainRecord DummyRecord() {
            var plugin = session.pluginManager.CreateDummyPlugin("rs.esp");
            var header = (esper.elements.MainRecord)plugin.GetElement("TES4");
            session.pluginManager.UnloadPlugin(plugin);
            return header;
        }

        [Test]
        public void TestReadSubrecordAdvances() {
            var rec = DummyRecord();
            var data = new byte[] {
                (byte)'E', (byte)'D', (byte)'I', (byte)'D',
                0x03, 0x00, (byte)'a', (byte)'b', (byte)'c'
            };
            var source = new RecordSource(rec, data);
            source.ReadSubrecord();
            Assert.AreEqual("EDID", source.currentSubrecord.signature.ToString());
            Assert.AreEqual(3u, source.currentSubrecord.dataSize);
        }

        [Test]
        public void TestXxxxSubrecordUsesNextSize() {
            var rec = DummyRecord();
            var data = new byte[] {
                (byte)'X', (byte)'X', (byte)'X', (byte)'X',
                0x04, 0x00, // XXXX size (discarded)
                0x09, 0x00, 0x00, 0x00, // nextSize = 9
                (byte)'E', (byte)'D', (byte)'I', (byte)'D',
                0x09, 0x00,
                (byte)'1', (byte)'2', (byte)'3', (byte)'4',
                (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9'
            };
            var source = new RecordSource(rec, data);
            // the XXXX escape is consumed transparently: one read
            // yields the real subrecord with the overridden size
            source.ReadSubrecord();
            Assert.AreEqual("EDID", source.currentSubrecord.signature.ToString());
            Assert.AreEqual(9u, source.currentSubrecord.dataSize);
        }

        [Test]
        public void TestOverReadThrowsWithPositions() {
            var rec = DummyRecord();
            var data = new byte[] {
                (byte)'E', (byte)'D', (byte)'I', (byte)'D',
                0x02, 0x00, // declared size smaller than payload
                (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e'
            };
            var source = new RecordSource(rec, data);
            source.ReadSubrecord();
            // consume past the declared subrecord end, then handling
            // the subrecord reports the over-read with positions
            source.reader.ReadBytes(5);
            var ex = Assert.Throws<Exception>(
                () => source.SubrecordHandled()
            );
            StringAssert.Contains("Read past", ex.Message);
            StringAssert.Contains("EDID", ex.Message);
        }

        [Test]
        public void TestOptionalStructFieldsSkipValuesThatDoNotFit() {
            var def = new StructDef(
                session.definitionManager,
                JObject.Parse(@"{
                    'type': 'struct',
                    'name': 'Versioned data',
                    'optionalFromElement': 0,
                    'elements': [
                        { 'type': 'float', 'name': 'Present' },
                        { 'type': 'float', 'name': 'Omitted' },
                        { 'type': 'uint16', 'name': 'Damage' }
                    ]
                }")
            );
            var payload = BitConverter.GetBytes(1.5f)
                .Concat(new byte[] { 0x34, 0x12 })
                .ToArray();
            var data = new byte[] {
                (byte)'D', (byte)'A', (byte)'T', (byte)'A',
                (byte)payload.Length, 0
            }.Concat(payload).ToArray();
            var source = new RecordSource(DummyRecord(), data);
            source.ReadSubrecord();

            var element = (StructElement)def.ReadElement(
                null, source, (uint)payload.Length
            );

            CollectionAssert.AreEqual(
                new[] { "Present", "Damage" },
                element.elements.Select(e => e.name)
            );
            Assert.AreEqual(1.5f, ((esper.defs.ValueData.F32)((ValueElement)element.elements[0]).data).V);
            Assert.AreEqual(0x1234, ((esper.defs.ValueData.U16)((ValueElement)element.elements[1]).data).V);
        }
    }
}
