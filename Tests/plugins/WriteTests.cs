using esper;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    public class WriteTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions { });
        }

        [Test]
        public void TestSave() {
            var oldPath = TestHelpers.FixturePath("AllRecords.esp");
            var plugin = pluginManager.LoadPlugin(oldPath);
            Assert.IsNotNull(plugin);
            var newPath = TestHelpers.FixturePath("Save.esp");
            plugin.Save(newPath);
            TestHelpers.BinaryEqual(oldPath, newPath);
        }
    }
}
