using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.IO;

namespace Tests.plugins {
    public class DragonbornEsmTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        private void LoadDragonbornEsm() {
            var pluginPath = TestHelpers.FixturePath("Dragonborn.esm");
            if (!File.Exists(pluginPath))
                Assert.Ignore("Local master fixture not found: Dragonborn.esm");
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            LoadDragonbornEsm();
        }

        [Test]
        public void TestLoad() {
            Assert.IsNotNull(plugin);
        }
    }
}
