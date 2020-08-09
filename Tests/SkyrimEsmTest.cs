using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.IO;

namespace Tests {
    public class SkyrimEsmTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public string pluginPath;
        public PluginFile plugin;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {
                readAllSubrecords = false
            });
            pluginPath = TestHelpers.FixturePath("Skyrim.esm");
            Assert.IsTrue(File.Exists(pluginPath),
                "Put Skyrim.esm in the fixtures directory to run these tests."
            );
        }

        [Test]
        public void TestLoad() {
            plugin = pluginManager.LoadPlugin(pluginPath);
            Assert.IsNotNull(plugin);
        }
    }
}
