using esper;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests {
    public class ARecordsTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {
                buildDefsOnDemand = true
            });
            var pluginPath = TestHelpers.FixturePath("ARecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
            Assert.IsNotNull(plugin);
        }

        [Test]
        public void TestActionRecord() {
            var rec = plugin.GetElement(@"AACT\[0]");
            Assert.IsNotNull(rec);
            TestHelpers.TestValue(rec, "EDID", "TestAction");
            TestHelpers.TestColor(rec, "CNAM");
        }

        [Test]
        public void TestActivatorRecord() {
            var rec = plugin.GetElement(@"ACTI\[0]");
            Assert.IsNotNull(rec);
            TestHelpers.TestValue(rec, "EDID", "TestActivator");
            TestHelpers.TestVMAD(rec, TestHelpers.ExpectAllProps);
            TestHelpers.TestObjectBounds(rec);
            TestHelpers.TestValue(rec, "FULL", "Test Activator");
            TestHelpers.TestModel(rec);
            TestHelpers.TestDestructible(rec);
            TestHelpers.TestKeywords(rec);
            TestHelpers.TestColor(rec, "PNAM");
            TestHelpers.TestFormId(rec, "SNAM");
            TestHelpers.TestFormId(rec, "VNAM");
            TestHelpers.TestFormId(rec, "WNAM");
            TestHelpers.TestValue(rec, "RNAM", "Test");
            TestHelpers.TestValue(rec, "FNAM", "No Displacement, Ignored by Sandbox");
            TestHelpers.TestFormId(rec, "KNAM");
        }

        [Test]
        public void TestAddonNodeRecord() {
            // TODO
        }

        [Test]
        public void TestIngestibleRecord() {
            // TODO
        }

        [Test]
        public void TestAmmunitionRecord() {
            // TODO
        }

        [Test]
        public void TestAnimatedObjectRecord() {
            // TODO
        }

        [Test]
        public void TestApparatusRecord() {
            // TODO
        }

        [Test]
        public void TestArmatureRecord() {
            // TODO
        }

        [Test]
        public void TestArmorRecord() {
            // TODO
        }

        [Test]
        public void TestArtObjectRecord() {
            // TODO
        }
    }
}
