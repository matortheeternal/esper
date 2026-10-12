using esper;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests.defs {
    // Epsilon boundary for negative-float sort keys, via real record
    // elements: FloatDef.DataToSortKey emits '-' only when
    // (data < 0) && (abs(data) > epsilon), with epsilon ~1e-6 at the
    // default floatDigits=6. A regression to +Infinity (the old
    // always-true uint-wrap bug) would flip every negative to '+'.
    public class FloatSortKeyTests {
        private static byte[] Subrecord(
            string sig, byte[] payload, ushort? sizeOverride = null
        ) {
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(sig));
            var size = sizeOverride ?? (ushort)payload.Length;
            bytes.AddRange(BitConverter.GetBytes(size));
            bytes.AddRange(payload);
            return bytes.ToArray();
        }

        private static byte[] Edid(string edid) {
            return Subrecord("EDID", Encoding.ASCII.GetBytes(edid + "\0"));
        }

        private static byte[] Record(
            string sig, uint formId, params byte[][] subrecords
        ) {
            var content = subrecords.SelectMany(b => b).ToArray();
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(sig));
            bytes.AddRange(BitConverter.GetBytes((uint)content.Length));
            bytes.AddRange(BitConverter.GetBytes(0u));
            bytes.AddRange(BitConverter.GetBytes(formId));
            bytes.AddRange(new byte[8]);
            bytes.AddRange(content);
            return bytes.ToArray();
        }

        private static byte[] Group(
            byte[] label, int type, params byte[][] records
        ) {
            var content = records.SelectMany(b => b).ToArray();
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("GRUP"));
            bytes.AddRange(BitConverter.GetBytes((uint)(content.Length + 24)));
            bytes.AddRange(label);
            bytes.AddRange(BitConverter.GetBytes(type));
            bytes.AddRange(BitConverter.GetBytes(0u));
            bytes.AddRange(BitConverter.GetBytes(0u));
            bytes.AddRange(content);
            return bytes.ToArray();
        }

        private static string WritePlugin(byte[] dataPosRot) {
            var bytes = new List<byte>();
            var header = new List<byte>();
            header.AddRange(Subrecord("HEDR", new byte[] {
                0x9A, 0x99, 0xD9, 0x3F, 0x01, 0x00, 0x00, 0x00,
                0x08, 0x00, 0x00, 0x00
            }));
            header.AddRange(Subrecord("CNAM", Encoding.ASCII.GetBytes("SortKey\0")));
            bytes.AddRange(Record("TES4", 0, header.ToArray()));
            // PFLA has no top group: placed refs nest under a CELL in
            // block/sub-block groups with a type-6 wrapper and a
            // persistent-children group labelled by the cell formId.
            var pfla = new List<byte>();
            pfla.AddRange(Edid("SortKeyPFLA"));
            pfla.AddRange(Subrecord("DATA", dataPosRot));
            var cell = new List<byte>();
            cell.AddRange(Subrecord("DATA", new byte[2]));
            cell.AddRange(Subrecord("LTMP", new byte[4]));
            bytes.AddRange(Group(
                Encoding.ASCII.GetBytes("CELL"), 0,
                Group(BitConverter.GetBytes(0), 2,
                Group(BitConverter.GetBytes(0), 3,
                    Record("CELL", 0x800, cell.ToArray()),
                    Group(BitConverter.GetBytes(0x800), 6,
                    Group(BitConverter.GetBytes(0x800), 8,
                        Record("PFLA", 0x80D, pfla.ToArray())))))
            ));
            var path = Path.Combine(
                Path.GetTempPath(), Guid.NewGuid() + ".esp"
            );
            File.WriteAllBytes(path, bytes.ToArray());
            return path;
        }

        private static string SortKeyOf(float x) {
            var floats = new float[] { x, 0f, 2.25f, -1e-9f, 0f, 0f };
            var payload = floats.SelectMany(BitConverter.GetBytes).ToArray();
            var path = WritePlugin(payload);
            try {
                var session = new Session(Games.SSE, new SessionOptions());
                var plugin = session.pluginManager.LoadPlugin(path);
                var rec = plugin.records.First(
                    r => r.signature.ToString() == "PFLA"
                );
                var el = rec.GetElement(@"DATA\Position\X");
                return el.sortKey;
            } finally {
                File.Delete(path);
            }
        }

        [Test]
        public void TestNegativeFloatSortsNegative() {
            var key = SortKeyOf(-1.5f);
            Assert.AreEqual(23, key.Length);
            Assert.AreEqual('-', key[0]);
        }

        [Test]
        public void TestZeroSortsPositive() {
            var key = SortKeyOf(0f);
            Assert.AreEqual(23, key.Length);
            Assert.AreEqual('+', key[0]);
        }

        [Test]
        public void TestSubEpsilonNegativeSortsPositive() {
            // |-1e-9| < epsilon (~1e-6): treated as zero
            var floats = new float[] { 0f, 0f, 2.25f, -1e-9f, 0f, 0f };
            var payload = floats.SelectMany(BitConverter.GetBytes).ToArray();
            var path = WritePlugin(payload);
            try {
                var session = new Session(Games.SSE, new SessionOptions());
                var plugin = session.pluginManager.LoadPlugin(path);
                var rec = plugin.records.First(
                    r => r.signature.ToString() == "PFLA"
                );
                var el = rec.GetElement(@"DATA\Rotation\X");
                Assert.AreEqual('+', el.sortKey[0]);
            } finally {
                File.Delete(path);
            }
        }
    }
}
