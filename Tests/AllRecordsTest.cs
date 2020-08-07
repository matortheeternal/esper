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
            session = new Session(Games.TES5, new SessionOptions {
                buildDefsOnDemand = true
            });
            var pluginPath = TestHelpers.FixturePath("AllRecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
            Assert.IsNotNull(plugin);
        }

        [Test]
        public void TestGroups() {
            Assert.AreEqual(57, plugin.elements.Count);
        }
    }
}
