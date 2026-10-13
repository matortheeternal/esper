using esper;
using esper.elements;
using esper.resolution;
using NUnit.Framework;

namespace Tests.plugins {
    // Covers SSE record signatures that have no AllRecords.esp fixture or
    // expected-value JSON (see fixtures/AllRecords/): ACHR, INFO, LAND,
    // NAVI, NAVM, REFR, PGRE and PHZD from Skyrim.esm, VOLI from
    // Dragonborn.esm, and LENS, PMIS and SCOL from Fallout4.esm.
    // Records resolve through Fixture.Record in all three test modes:
    // fixture plugins by default, game ESMs in GAME mode, ESM fallback
    // with sidecar growth in BUILD mode.
    [TestFixture]
    public class MissingRecordsTests {
        [Test]
        public void TestACHR() {
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0xEA7);
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
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0xDD7);
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
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0xEE7);
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
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0x12FB4);
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
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0xE15);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:056C1B}");
            Assert.AreEqual(12, rec.GetData<uint>(@"NVNM\Version"));
            TestHelpers.TestValue(rec, @"NVNM\Magic", "3C A0 E9 A5");
        }

        [Test]
        public void TestREFR() {
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0xDD1);
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
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0x4C1D7);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:0097E8}");
            TestHelpers.TestValue(rec, "NAME", "{Skyrim.esm:07326E}");
            TestHelpers.TestValue(rec, @"Record Header\Form Version", "35");
            Assert.AreEqual(35126.035156f,
                rec.GetData<float>(@"DATA\Position\X"), 0.01);
        }

        [Test]
        public void TestPHZD() {
            var rec = Fixture.Record(Games.SSE, "Skyrim.esm", 0x1FB1C);
            TestHelpers.TestValue(rec, "Cell", "{Skyrim.esm:0243DF}");
            TestHelpers.TestValue(rec, "NAME", "{Skyrim.esm:0153BF}");
            TestHelpers.TestValue(rec, @"Record Header\Form Version", "39");
            Assert.AreEqual(0.83f,
                rec.GetData<float>("XSCL"), 0.0001);
        }

        [Test]
        public void TestVOLI() {
            var rec = Fixture.Record(Games.SSE, "Dragonborn.esm", 0x201E2CD);
            TestHelpers.TestValue(rec, "EDID", "VLDLC2Ash01Day");
            Assert.AreEqual(2.0f, rec.GetData<float>("CNAM"), 0.0001);
        }

        [Test]
        public void TestLENS() {
            var rec = Fixture.Record(Games.FO4, "Fallout4.esm", 0x16417D);
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
            var rec = Fixture.Record(Games.FO4, "Fallout4.esm", 0x17309F);
            TestHelpers.TestValue(rec, "Cell", "{Fallout4.esm:00E30D}");
            TestHelpers.TestValue(rec, "NAME", "{Fallout4.esm:0CABA4}");
            Assert.AreEqual(107281.0625f,
                rec.GetData<float>(@"DATA\Position\X"), 0.1);
        }

        [Test]
        public void TestSCOL() {
            var rec = Fixture.Record(Games.FO4, "Fallout4.esm", 0x17F6D);
            TestHelpers.TestValue(rec, "EDID", "IndustrialMachine48");
            Assert.AreEqual(-39, rec.GetData<short>(@"OBND\X1"));
            Assert.AreEqual(515, rec.GetData<short>(@"OBND\X2"));
            TestHelpers.TestValue(rec, @"Model\MODL",
                @"SCOL\Fallout4.esm\CM00017F6D.NIF");
        }
    }
}
