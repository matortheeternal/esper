using esper;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests.plugins {
    // Covers the SSE record signatures that appear in no available master
    // file (CLDC, HAIR, PARW, PBAR, PBEA, PCON, PFLA, PLYR, PWAT, RGDL and
    // SCPT) plus synthetic LENS and SCOL records. The plugin binary is
    // hand-crafted in this file, so it acts as an independent oracle for
    // both the reader and the writer: loading, value decoding, saving,
    // reloading and byte-identical round trips are all asserted.
    // Only temp files are used; nothing is committed.
    [TestFixture]
    public class SyntheticRecordsTests {
        private Session session;
        private string synthPath;

        private static byte[] Subrecord(string sig, byte[] payload) {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(Encoding.ASCII.GetBytes(sig));
            w.Write((ushort)payload.Length);
            w.Write(payload);
            return ms.ToArray();
        }

        private static byte[] ZString(string s) {
            return Encoding.ASCII.GetBytes(s + "\0");
        }

        private static byte[] Record(string sig, uint formId, byte[][] subs) {
            var body = subs.SelectMany(b => b).ToArray();
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(Encoding.ASCII.GetBytes(sig));
            w.Write((uint)body.Length);
            w.Write(0u); // flags
            w.Write(formId);
            w.Write(new byte[8]);
            w.Write(body);
            return ms.ToArray();
        }

        private static byte[] Group(
            int groupType, byte[] label, params byte[][] children
        ) {
            var body = children.SelectMany(b => b).ToArray();
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(Encoding.ASCII.GetBytes("GRUP"));
            w.Write((uint)(24 + body.Length));
            w.Write(label);
            w.Write(groupType);
            w.Write(0u); // version control info
            w.Write(0u); // unknown
            w.Write(body);
            return ms.ToArray();
        }

        private static byte[] Edid(string edid) {
            return Subrecord("EDID", ZString(edid));
        }

        private static byte[] BuildPlugin() {
            using var ms = new MemoryStream();
            // File header: HEDR + CNAM only, no masters.
            using (var w = new BinaryWriter(ms, Encoding.ASCII, true)) {
                var hedr = new byte[12];
                BitConverter.GetBytes(1.7f).CopyTo(hedr, 0);
                BitConverter.GetBytes(14u).CopyTo(hedr, 4);
                BitConverter.GetBytes(0x80Eu).CopyTo(hedr, 8);
                var header = Record("TES4", 0, new[] {
                    Subrecord("HEDR", hedr),
                    Subrecord("CNAM", ZString("Synthetic")),
                });
                w.Write(header);
            }
            var groups = new[] {
                Group(0, Encoding.ASCII.GetBytes("HAIR"),
                    Record("HAIR", 0x800, new[] { Edid("TestHair") })),
                Group(0, Encoding.ASCII.GetBytes("SCPT"),
                    Record("SCPT", 0x801, new[] { Edid("TestScript") })),
                Group(0, Encoding.ASCII.GetBytes("CLDC"),
                    Record("CLDC", 0x802, new[] { Edid("TestCLDC") })),
                Group(0, Encoding.ASCII.GetBytes("PWAT"),
                    Record("PWAT", 0x803, new[] { Edid("TestPWAT") })),
                Group(0, Encoding.ASCII.GetBytes("RGDL"),
                    Record("RGDL", 0x804, new[] { Edid("TestRGDL") })),
                Group(0, Encoding.ASCII.GetBytes("PLYR"),
                    Record("PLYR", 0x805, new[] {
                        Edid("TestPlayer"),
                        Subrecord("PLYR", BitConverter.GetBytes(7u)),
                    })),
                Group(0, Encoding.ASCII.GetBytes("LENS"),
                    Record("LENS", 0x806, BuildLens())),
                Group(0, Encoding.ASCII.GetBytes("SCOL"),
                    Record("SCOL", 0x807, BuildScol())),
                Group(0, Encoding.ASCII.GetBytes("CELL"),
                    Group(2, BitConverter.GetBytes(0),
                        Group(3, BitConverter.GetBytes(0),
                            Record("CELL", 0x808, new[] {
                                Subrecord("DATA", new byte[2]),
                                Subrecord("LTMP", new byte[4]),
                            }),
                            Group(6, BitConverter.GetBytes(0x808),
                            Group(8, BitConverter.GetBytes(0x808),
                                Record("PARW", 0x809,
                                    new[] { Edid("TestPARW"),
                                        Subrecord("DATA", new byte[24]) }),
                                Record("PBAR", 0x80A,
                                    new[] { Edid("TestPBAR"),
                                        Subrecord("DATA", new byte[24]) }),
                                Record("PBEA", 0x80B,
                                    new[] { Edid("TestPBEA"),
                                        Subrecord("DATA", new byte[24]) }),
                                Record("PCON", 0x80C,
                                    new[] { Edid("TestPCON"),
                                        Subrecord("DATA", new byte[24]) }),
                                Record("PFLA", 0x80D,
                                    new[] { Edid("TestPFLA"),
                                        Subrecord("DATA", new byte[24]) })))))),
            };
            foreach (var g in groups) ms.Write(g, 0, g.Length);
            return ms.ToArray();
        }

        private static byte[][] BuildLens() {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(1.0f); w.Write(0.5f); w.Write(0.25f); // tint
            w.Write(10.0f); // width
            w.Write(20.0f); // height
            w.Write(0.5f); // position
            w.Write(1.0f); // angular fade
            w.Write(0.8f); // opacity
            w.Write(1u); // flags: Rotates
            var lfsd = ms.ToArray();
            return new[] {
                Edid("TestLens"),
                Subrecord("CNAM", BitConverter.GetBytes(0.5f)),
                Subrecord("DNAM", BitConverter.GetBytes(100.0f)),
                Subrecord("LFSP", BitConverter.GetBytes(1u)),
                Subrecord("DNAM", ZString("Circle")),
                Subrecord("FNAM", ZString("flare.dds")),
                Subrecord("LFSD", lfsd),
            };
        }

        private static byte[][] BuildScol() {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write((short)-1); w.Write((short)-3); w.Write((short)-5);
            w.Write((short)2); w.Write((short)4); w.Write((short)6);
            var obnd = ms.ToArray();
            using var ms2 = new MemoryStream();
            using var w2 = new BinaryWriter(ms2);
            w2.Write(1.0f); w2.Write(2.0f); w2.Write(3.0f); // position
            w2.Write(0.0f); w2.Write(0.0f); w2.Write(0.0f); // rotation
            w2.Write(1.0f); // scale
            var placement = ms2.ToArray();
            return new[] {
                Edid("TestScol"),
                Subrecord("OBND", obnd),
                Subrecord("MODL", ZString("test.nif")),
                Subrecord("ONAM", new byte[4]),
                Subrecord("DATA", placement),
            };
        }

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            synthPath = Path.Combine(
                Path.GetTempPath(), $"EsperSynth{Guid.NewGuid():N}.esp");
            File.WriteAllBytes(synthPath, BuildPlugin());
        }

        [OneTimeTearDown]
        public void TearDown() {
            if (synthPath != null && File.Exists(synthPath))
                File.Delete(synthPath);
        }

        private PluginFile Load() {
            return session.pluginManager.LoadPlugin(synthPath);
        }

        [Test]
        public void TestAllRecordsLoad() {
            var plugin = Load();
            Assert.AreEqual(14, plugin.records.Count);
            foreach (var sig in new[] {
                "HAIR", "SCPT", "CLDC", "PWAT", "RGDL", "PLYR",
                "LENS", "SCOL", "CELL",
                "PARW", "PBAR", "PBEA", "PCON", "PFLA",
            }) {
                var rec = plugin.records.FirstOrDefault(
                    r => r.signature.ToString() == sig);
                Assert.IsNotNull(rec, $"Missing {sig} record.");
                Assert.IsTrue(rec.elements.Count > 0);
            }
        }

        [Test]
        public void TestSimpleEdids() {
            var plugin = Load();
            foreach (var (sig, edid) in new[] {
                ("HAIR", "TestHair"), ("SCPT", "TestScript"),
                ("CLDC", "TestCLDC"), ("PWAT", "TestPWAT"),
                ("RGDL", "TestRGDL"), ("PARW", "TestPARW"),
                ("PBAR", "TestPBAR"), ("PBEA", "TestPBEA"),
                ("PCON", "TestPCON"), ("PFLA", "TestPFLA"),
            }) {
                var rec = plugin.records.First(
                    r => r.signature.ToString() == sig);
                TestHelpers.TestValue(rec, "EDID", edid);
            }
        }

        [Test]
        public void TestPlayer() {
            var plugin = Load();
            var rec = plugin.GetRecordByFormId(0x805);
            Assert.IsNotNull(rec);
            TestHelpers.TestValue(rec, "EDID", "TestPlayer");
            StringAssert.EndsWith(":000007}", rec.GetValue("Player"));
        }

        [Test]
        public void TestLens() {
            var plugin = Load();
            var rec = plugin.GetRecordByFormId(0x806);
            Assert.IsNotNull(rec);
            TestHelpers.TestValue(rec, "EDID", "TestLens");
            Assert.AreEqual(0.5f, rec.GetData<float>("CNAM"), 0.0001);
            Assert.AreEqual(100.0f, rec.GetData<float>("DNAM"), 0.0001);
            Assert.AreEqual(1, rec.GetData<uint>("LFSP"));
            var flare = rec.GetElements("Lens Flare Sprites")[0];
            TestHelpers.TestValue(flare, "DNAM", "Circle");
            TestHelpers.TestValue(flare, "FNAM", "flare.dds");
            Assert.AreEqual(1.0f,
                flare.GetData<float>(@"LFSD\Tint\Red"), 0.0001);
            Assert.AreEqual(10.0f,
                flare.GetData<float>(@"LFSD\Width"), 0.0001);
            TestHelpers.TestValue(flare, @"LFSD\Flags", "Rotates");
        }

        [Test]
        public void TestScol() {
            var plugin = Load();
            var rec = plugin.GetRecordByFormId(0x807);
            Assert.IsNotNull(rec);
            TestHelpers.TestValue(rec, "EDID", "TestScol");
            Assert.AreEqual(-1, rec.GetData<short>(@"OBND\X1"));
            Assert.AreEqual(6, rec.GetData<short>(@"OBND\Z2"));
            TestHelpers.TestValue(rec, @"Model\MODL", "test.nif");
            var part = rec.GetElements("Parts")[0];
            Assert.AreEqual(1.0f,
                part.GetData<float>(@"DATA\[0]\Position\X"), 0.0001);
            Assert.AreEqual(3.0f,
                part.GetData<float>(@"DATA\[0]\Position\Z"), 0.0001);
            Assert.AreEqual(1.0f,
                part.GetData<float>(@"DATA\[0]\Scale"), 0.0001);
        }

        [Test]
        public void TestRequiredMemberMaterializes() {
            // PFLA without its required DATA subrecord: decode
            // materializes it with default (zero) data, and saving
            // persists the materialized subrecord.
            using var ms = new MemoryStream();
            using (var w = new BinaryWriter(ms, Encoding.ASCII, true)) {
                var hedr = new byte[12];
                BitConverter.GetBytes(1.7f).CopyTo(hedr, 0);
                BitConverter.GetBytes(2u).CopyTo(hedr, 4);
                BitConverter.GetBytes(0x802u).CopyTo(hedr, 8);
                w.Write(Record("TES4", 0, new[] {
                    Subrecord("HEDR", hedr),
                    Subrecord("CNAM", ZString("Synthetic")),
                }));
                w.Write(Group(0, Encoding.ASCII.GetBytes("CELL"),
                    Group(2, BitConverter.GetBytes(0),
                        Group(3, BitConverter.GetBytes(0),
                            Record("CELL", 0x800, new[] {
                                Subrecord("DATA", new byte[2]),
                                Subrecord("LTMP", new byte[4]),
                            }),
                            Group(6, BitConverter.GetBytes(0x800),
                            Group(8, BitConverter.GetBytes(0x800),
                                Record("PFLA", 0x801,
                                    new[] {
                                        Edid("NoDataPFLA") })))))));
            }
            var path = Path.Combine(Path.GetTempPath(),
                $"EsperSynth{Guid.NewGuid():N}.esp");
            var savePath = Path.Combine(Path.GetTempPath(),
                $"EsperSynth{Guid.NewGuid():N}.esp");
            try {
                File.WriteAllBytes(path, ms.ToArray());
                var plugin = session.pluginManager.LoadPlugin(path);
                var rec = plugin.GetRecordByFormId(0x801);
                Assert.IsNotNull(rec);
                var data = rec.GetElement("DATA");
                Assert.IsNotNull(data);
                Assert.AreEqual(0.0f,
                    data.GetData<float>(@"Position\X"), 0.0001);
                plugin.Save(savePath);
                var reloaded =
                    session.pluginManager.LoadPlugin(savePath);
                var rec2 = reloaded.GetRecordByFormId(0x801);
                TestHelpers.TestValue(rec2, "EDID", "NoDataPFLA");
                Assert.AreEqual(0.0f,
                    rec2.GetData<float>(@"DATA\Position\Z"), 0.0001);
            } finally {
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(savePath)) File.Delete(savePath);
            }
        }

        [Test]
        public void TestSaveReloadRoundTrip() {
            var plugin = Load();
            // Force every record to decode before saving.
            foreach (var rec in plugin.records) {
                Assert.IsTrue(rec.elements.Count > 0);
                rec.elements.ToList();
            }
            var savePath = Path.Combine(Path.GetTempPath(),
                $"EsperSynth{Guid.NewGuid():N}.esp");
            try {
                plugin.Save(savePath);
                TestHelpers.BinaryEqual(synthPath, savePath);
                var reloaded =
                    session.pluginManager.LoadPlugin(savePath);
                Assert.AreEqual(14, reloaded.records.Count);
                var lens = reloaded.GetRecordByFormId(0x806);
                TestHelpers.TestValue(lens, "EDID", "TestLens");
            } finally {
                if (File.Exists(savePath)) File.Delete(savePath);
            }
        }
    }
}
