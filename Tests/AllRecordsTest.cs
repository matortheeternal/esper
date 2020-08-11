using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;

namespace Tests {
    public class AllRecordsTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
        }

        [Test]
        public void TestGroups() {
            var pluginPath = TestHelpers.FixturePath("AllRecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
            Assert.IsNotNull(plugin);
            Assert.AreEqual(112, plugin.elements.Count);
        }
    }
}
