using esper;
using esper.parsing;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System;

namespace Tests {
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
            Assert.IsNotNull(fileHeader.header);
            var sig = fileHeader.GetValue(@"Record Header\Signature");
            Assert.AreEqual(sig, "TES4");
            var dataSize = fileHeader.GetValue(@"Record Header\Data Size");
            Assert.AreEqual(dataSize, "30");
            var flags = fileHeader.GetValue(@"Record Header\Record Flags");
            Assert.AreEqual(flags, "0"); // ""
            var formId = fileHeader.GetValue(@"Record Header\FormID");
            Assert.AreEqual(formId, "{Hardcoded:000000}"); // "NULL - Null Reference [00000000]"
            var vc1 = fileHeader.GetValue(@"Record Header\Version Control Info 1");
            Assert.AreEqual(vc1, "00 00 00 00");
            var formVersion = fileHeader.GetValue(@"Record Header\Form Version");
            Assert.AreEqual(formVersion, "43");
            var vc2 = fileHeader.GetValue(@"Record Header\Version Control Info 2");
            Assert.AreEqual(vc2, "00 00");
        }

        [Test]
        public void TestRecordHeaderData() {
            var fileHeader = plugin.header;
            Assert.IsNotNull(fileHeader);
            Assert.IsNotNull(fileHeader.header);
            string sig = fileHeader.GetData(@"Record Header\Signature");
            Assert.AreEqual(sig, "TES4");
            UInt32 dataSize = fileHeader.GetData(@"Record Header\Data Size");
            Assert.AreEqual(dataSize, 30);
            UInt32 flags = fileHeader.GetData(@"Record Header\Record Flags");
            Assert.AreEqual(flags, 0);
            FormId formId = fileHeader.GetData(@"Record Header\FormID");
            Assert.AreEqual(formId.localFormId, 0);
            byte[] vc1 = fileHeader.GetData(@"Record Header\Version Control Info 1");
            Assert.AreEqual(vc1[0], 0);
            UInt16 formVersion = fileHeader.GetData(@"Record Header\Form Version");
            Assert.AreEqual(formVersion, 43);
            byte[] vc2 = fileHeader.GetData(@"Record Header\Version Control Info 2");
            Assert.AreEqual(vc2[0], 0);
        }
    }
}
