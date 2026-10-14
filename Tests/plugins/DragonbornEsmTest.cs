using esper;
using esper.plugins;
using NUnit.Framework;

namespace Tests.plugins {
    public class DragonbornEsmTest {
        public PluginFile plugin;

        private void LoadDragonbornEsm() {
            plugin = Fixture.Master(Games.SSE, "Dragonborn.esm");
        }

        [OneTimeSetUp]
        public void SetUp() {
            LoadDragonbornEsm();
        }

        [Test]
        public void TestLoad() {
            Assert.IsNotNull(plugin);
        }
    }
}
