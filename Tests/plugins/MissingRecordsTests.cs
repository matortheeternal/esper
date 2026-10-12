using esper;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System.IO;

namespace Tests.plugins {
    // Covers SSE record signatures that have no AllRecords.esp fixture or
    // expected-value JSON (see fixtures/AllRecords/): ACHR, INFO, LAND,
    // NAVI, NAVM, REFR, PGRE and PHZD from Skyrim.esm, VOLI from
    // Dragonborn.esm, and LENS, PMIS and SCOL from Fallout4.esm.
    // Master files are local-only fixtures, so every test ignores itself
    // when the required master is absent.
    [TestFixture]
    public class MissingRecordsTests {
        private Session sseSession;
        private PluginFile skyrim;
        private PluginFile dragonborn;
        private Session fo4Session;
        private PluginFile fallout4;

        private PluginFile LoadOptional(Session session, string filename) {
            var path = TestHelpers.FixturePath(filename);
            if (!File.Exists(path)) return null;
            return session.pluginManager.LoadPlugin(path);
        }

        [OneTimeSetUp]
        public void SetUp() {
            sseSession = new Session(Games.SSE, new SessionOptions());
            skyrim = LoadOptional(sseSession, "Skyrim.esm");
            dragonborn = LoadOptional(sseSession, "Dragonborn.esm");
            fo4Session = new Session(Games.FO4, new SessionOptions());
            fallout4 = LoadOptional(fo4Session, "Fallout4.esm");
        }

        private MainRecord SkyrimRec(uint formId) {
            if (skyrim == null)
                Assert.Ignore("Skyrim.esm is not present.");
            var rec = skyrim.GetRecordByFormId(formId);
            Assert.IsNotNull(rec);
            return rec;
        }

        private MainRecord DragonbornRec(uint formId) {
            if (dragonborn == null)
                Assert.Ignore("Dragonborn.esm is not present.");
            var rec = dragonborn.GetRecordByFormId(formId);
            Assert.IsNotNull(rec);
            return rec;
        }

        private MainRecord Fallout4Rec(uint formId) {
            if (fallout4 == null)
                Assert.Ignore("Fallout4.esm is not present.");
            var rec = fallout4.GetRecordByFormId(formId);
            Assert.IsNotNull(rec);
            return rec;
        }

        [Test]
        public void TestACHR() {
            var rec = SkyrimRec(0xEA7);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:0961FF}");
            TestHelpers.TestValue(rec, "NAME", "{Skyrim.esm:000EA6}");
            Assert.AreEqual(-1387.257202f,
                rec.GetData<float>(@"DATA\Position\X"), 0.001);
            Assert.AreEqual(-2425.269287f,
                rec.GetData<float>(@"DATA\Position\Y"), 0.001);
            Assert.AreEqual(6487.40332f,
                rec.GetData<float>(@"DATA\Position\Z"), 0.001);
        }

        [Test]
        public void TestINFO() {
            var rec = SkyrimRec(0xDD7);
            TestHelpers.TestValue(rec, "DNAM", "{Skyrim.esm:06B155}");
            var conditions = rec.GetElements("Conditions");
            Assert.IsTrue(conditions.Count > 0);
            var condition = conditions[0] as Container;
            Assert.IsNotNull(condition);
            var ctda = condition.elements[0];
            TestHelpers.TestValue(ctda, "Function", "GetQuestRunning");
            TestHelpers.TestValue(
                ctda, "Parameter #1", "{Skyrim.esm:0D1444}");
            Assert.AreEqual(1.0f,
                ctda.GetData<float>("Comparison Value"), 0.0001);
        }

        [Test]
        public void TestLAND() {
            var rec = SkyrimRec(0xEE7);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:01A278}");
            var flags = rec.GetValue("DATA");
            StringAssert.Contains("Height Map", flags);
            StringAssert.Contains("Vertex Colours", flags);
            var recordFlags = rec.GetElement("Record Header")
                .GetValue("Record Flags");
            StringAssert.Contains("Compressed", recordFlags);
        }

        [Test]
        public void TestNAVI() {
            var rec = SkyrimRec(0x12FB4);
            Assert.AreEqual(12, rec.GetData<uint>("NVER"));
            var info = rec.GetElements("Navigation Map Infos")[0];
            Assert.IsNotNull(info);
            TestHelpers.TestValue(
                info, "Navigation Mesh", "{Skyrim.esm:100A27}");
            TestHelpers.TestValue(
                info, "Parent Worldspace", "{Skyrim.esm:00003C}");
        }

        [Test]
        public void TestNAVM() {
            var rec = SkyrimRec(0xE15);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:056C1B}");
            Assert.AreEqual(12, rec.GetData<uint>(@"NVNM\Version"));
            TestHelpers.TestValue(rec, @"NVNM\Magic", "3C A0 E9 A5");
        }

        [Test]
        public void TestREFR() {
            var rec = SkyrimRec(0xDD1);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:027552}");
            TestHelpers.TestValue(rec, "NAME", "{Skyrim.esm:068B7F}");
            TestHelpers.TestValue(rec, "XTRI", "47");
            TestHelpers.TestValue(rec, @"XPRM\Type", "Box");
            Assert.AreEqual(2820.855713f,
                rec.GetData<float>(@"DATA\Position\X"), 0.001);
            Assert.AreEqual(-227.638901f,
                rec.GetData<float>(@"DATA\Position\Y"), 0.001);
            Assert.AreEqual(37.328831f,
                rec.GetData<float>(@"DATA\Position\Z"), 0.001);
        }

        [Test]
        public void TestPGRE() {
            var rec = SkyrimRec(0x4C1D7);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:0097E8}");
            TestHelpers.TestValue(rec, "NAME", "{Skyrim.esm:07326E}");
            TestHelpers.TestValue(rec, @"Record Header\Form Version", "35");
            Assert.AreEqual(35126.035156f,
                rec.GetData<float>(@"DATA\Position\X"), 0.01);
        }

        [Test]
        public void TestPHZD() {
            var rec = SkyrimRec(0x1FB1C);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:0243DF}");
            TestHelpers.TestValue(rec, "NAME", "{Skyrim.esm:0153BF}");
            TestHelpers.TestValue(rec, @"Record Header\Form Version", "39");
            Assert.AreEqual(0.83f,
                rec.GetData<float>("XSCL"), 0.0001);
        }

        [Test]
        public void TestVOLI() {
            var rec = DragonbornRec(0x201E2CD);
            TestHelpers.TestValue(rec, "EDID", "VLDLC2Ash01Day");
            Assert.AreEqual(2.0f, rec.GetData<float>("CNAM"), 0.0001);
        }

        [Test]
        public void TestLENS() {
            var rec = Fallout4Rec(0x16417D);
            TestHelpers.TestValue(rec, "EDID", "FireFlare01Bright");
            Assert.AreEqual(5, rec.GetData<uint>("LFSP"));
            var sprite = rec.GetElements("Lens Flare Sprites")[0];
            Assert.IsNotNull(sprite);
            TestHelpers.TestValue(sprite, "DNAM", "Circle");
            TestHelpers.TestValue(sprite, "FNAM",
                @"Effects\LensFlares\GlowCircle01_d.DDS");
            Assert.AreEqual(1.0f,
                sprite.GetData<float>(@"LFSD\Tint\Red"), 0.0001);
        }

        [Test]
        public void TestPMIS() {
            var rec = Fallout4Rec(0x17309F);
            TestHelpers.TestValue(rec, "Cell", "{Fallout4.esm:00E30D}");
            TestHelpers.TestValue(rec, "NAME", "{Fallout4.esm:0CABA4}");
            Assert.AreEqual(107281.0625f,
                rec.GetData<float>(@"DATA\Position\X"), 0.1);
        }

        [Test]
        public void TestSCOL() {
            var rec = Fallout4Rec(0x17F6D);
            TestHelpers.TestValue(rec, "EDID", "IndustrialMachine48");
            Assert.AreEqual(-39, rec.GetData<short>(@"OBND\X1"));
            Assert.AreEqual(515, rec.GetData<short>(@"OBND\X2"));
            TestHelpers.TestValue(rec, @"Model\MODL",
                @"SCOL\Fallout4.esm\CM00017F6D.NIF");
        }
    }
}
