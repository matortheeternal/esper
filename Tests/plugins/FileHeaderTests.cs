using esper;
using esper.data;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    public class FileHeaderTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {
                buildDefsOnDemand = true
            });
            var pluginPath = TestHelpers.FixturePath("EmptyPlugin.esp");
            plugin = pluginManager.LoadPluginHeader(pluginPath);
            Assert.IsNotNull(plugin);
        }

        [Test]
        public void TestRecordHeaderValues() {
            var fileHeader = plugin.header;
            Assert.IsNotNull(fileHeader);
            var rh = fileHeader.GetElement("Record Header");
            Assert.IsNotNull(rh);
            Assert.AreEqual("TES4", rh.GetValue("Signature"));
            Assert.AreEqual("113", rh.GetValue("Data Size"));
            Assert.AreEqual("", rh.GetValue("Record Flags"));
            // "NULL - Null Reference [00000000]"
            Assert.AreEqual("{Null:000000}", rh.GetValue("FormID"));
            var vc1 = rh.GetValue("Version Control Info 1");
            Assert.AreEqual("00 00 00 00", vc1);
            Assert.AreEqual("44", rh.GetValue("Form Version"));
            Assert.AreEqual("00 00", rh.GetValue("Version Control Info 2"));
        }

        [Test]
        public void TestRecordHeaderData() {
            var fileHeader = plugin.header;
            var rh = fileHeader.GetElement("Record Header");
            Assert.AreEqual("TES4", rh.GetData("Signature"));
            Assert.AreEqual(113, rh.GetData("Data Size"));
            Assert.AreEqual(0, rh.GetData("Record Flags"));
            FormId formId = rh.GetData("FormID");
            Assert.AreEqual(formId.localFormId, 0);
            byte[] vc1 = rh.GetData("Version Control Info 1");
            Assert.AreEqual(vc1[0], 0);
            Assert.AreEqual(44, rh.GetData("Form Version"));
            byte[] vc2 = rh.GetData("Version Control Info 2");
            Assert.AreEqual(vc2[0], 0);
        }

        [Test]
        public void TestSubrecordValues() {
            var fh = plugin.header;
            var hedr = fh.GetElement("HEDR");
            Assert.AreEqual("1.700000", hedr.GetValue("Version"));
            Assert.AreEqual("0", hedr.GetValue("Number of Records"));
            Assert.AreEqual("000800", hedr.GetValue("Next Object ID"));
            Assert.AreEqual("", fh.GetValue("DELE"));
            Assert.AreEqual("Mator", fh.GetValue("CNAM"));
            Assert.AreEqual("An empty test plugin.", fh.GetValue("SNAM"));
            var ma = fh.GetElement(@"Master Files\[0]");
            Assert.AreEqual("Skyrim.esm", ma.GetValue("MAST"));
            Assert.AreEqual("00 00 00 00 00 00 00 00", ma.GetValue("DATA"));
            Assert.AreEqual("", fh.GetValue("SCRN"));
            Assert.AreEqual("", fh.GetValue("INTV"));
            Assert.AreEqual("", fh.GetValue("INCC"));
        }

    }
}
