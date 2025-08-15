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
            Assert.IsTrue(File.Exists(pluginPath),
                "See README.md for test fixture installation instructions."
            );
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
