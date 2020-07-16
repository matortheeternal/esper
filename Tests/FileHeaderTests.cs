using esper;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

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
        public void TestRecordHeader() {
            var fileHeader = plugin.header;
            Assert.IsNotNull(fileHeader);
            Assert.IsNotNull(fileHeader.header);
            var sig = fileHeader.GetValue(@"Record Header\Signature");
            Assert.AreEqual(sig, "TES4");
            var dataSize = fileHeader.GetValue(@"Record Header\Data Size");
            Assert.AreEqual(dataSize, "54");
            var flags = fileHeader.GetValue(@"Record Header\Record Flags");
            Assert.AreEqual(flags, "");
            var formId = fileHeader.GetValue(@"Record Header\FormID");
            Assert.AreEqual(formId, "NULL - Null Reference [00000000]");
            var vc1 = fileHeader.GetValue(@"Record Header\Version Control Info 1");
            Assert.AreEqual(vc1, "00 00 00 00");
            var formVersion = fileHeader.GetValue(@"Record Header\Form Version");
            Assert.AreEqual(formVersion, "43");
            var vc2 = fileHeader.GetValue(@"Record Header\Version Control Info 2");
            Assert.AreEqual(vc2, "00 00");
        }
    }
}
