using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Tests.plugins {
    public class SerializationTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public static PluginFile plugin;

        public void SetUp(bool serializeNumericData) {
            session = new Session(Games.SSE, new SessionOptions { serializeNumericData = serializeNumericData });
            var pluginPath = TestHelpers.FixturePath("AllRecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        [Test]
        public void TestJson() {
            SetUp(true);
            var json = plugin.ToJson();
            Assert.AreEqual("AllRecords.esp", json["Filename"].ToString());
            Assert.IsNotNull(json["File Header"]);
            Assert.IsNotNull(json["Armor"]);
            Assert.AreEqual(113, json.Count());
            File.WriteAllText(TestHelpers.FixturePath("AllRecords.json"), json.ToString());
        }
    }
}
