using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.IO;

namespace Tests {
    public class SkyrimEsmTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {
                readAllSubrecords = false
            });
        }

        [Test]
        public void TestLoad() {
            var pluginPath = TestHelpers.FixturePath("Skyrim.esm");
            Assert.IsTrue(File.Exists(pluginPath), 
                "Put Skyrim.esm in the fixtures directory to run this test."
            );
            plugin = pluginManager.LoadPlugin(pluginPath);
            Assert.IsNotNull(plugin);
        }
    }
}
