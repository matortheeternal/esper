using esper;
using esper.data;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    public class AllRecordsTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            var pluginPath = TestHelpers.FixturePath("AllRecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        [Test]
        public void TestGroups() {
            Assert.IsNotNull(plugin);
            Assert.AreEqual(112, plugin.elements.Count);
        }

        [Test]
        public void TestValues() {
            plugin.elements.ForEach(element => {
                if (element is MainRecord rec) {
                    JsonTestHelpers.TestJsonValues("File Header", element);
                } else if (element is GroupRecord group) {
                    Signature sig = group.GetLabel();
                    JsonTestHelpers.TestJsonValues(sig.ToString(), element);
                }
            });
        }
    }
}
