using esper;
using esper.resolution;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Tests.plugins {
    public class AllRecordsTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public static PluginFile plugin;

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

        public static string[] GetFixtures() {
            var folderPath = TestHelpers.FixturePath("AllRecords");
            return Directory.GetFiles(folderPath).Select(filePath => {
                return Path.GetFileNameWithoutExtension(filePath);
            }).ToArray();
        }

        [Test, TestCaseSource("GetFixtures")]
        public void TestValues(string path) {
            var element = plugin.GetElement(path);
            JsonTestHelpers.TestJsonValues(path, element);
        }
    }
}
