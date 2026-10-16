using esper;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    public class CreateElementTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [SetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {});
            var pluginPath = TestHelpers.FixturePath("EmptyPlugin.esp");
            plugin = pluginManager.LoadPluginHeader(pluginPath);
            Assert.IsNotNull(plugin);
        }

        [Test]
        public void TestAddGroupToFileReturnsAttachedGroup() {
            var before = plugin.elements.Count;
            var group = plugin.AddElement("ARMO") as GroupRecord;
            Assert.IsNotNull(group, "AddElement(file, sig) should return a group");
            Assert.AreSame(plugin, group.container,
                "new top group should be attached to the file");
            Assert.AreEqual(before + 1, plugin.elements.Count,
                "file should gain exactly one top-level child");
            Assert.AreSame(group, plugin.GetElement("ARMO"),
                "new group should be reachable by signature");
        }

        [Test]
        public void TestAddExistingGroupReusesGroup() {
            var first = plugin.AddElement("ARMO") as GroupRecord;
            var count = plugin.elements.Count;
            var second = plugin.AddElement("ARMO") as GroupRecord;
            Assert.IsNotNull(second);
            Assert.AreSame(first, second,
                "second add should reuse the existing group");
            Assert.AreEqual(count, plugin.elements.Count,
                "no duplicate top group should be created");
        }

        [Test]
        public void TestAddRecordInGroupByPath() {
            var rec = plugin.AddElement(@"ARMO\ARMO") as MainRecord;
            Assert.IsNotNull(rec, "AddElement(file, sig\\sig) should return a record");
            Assert.AreEqual("ARMO", rec.signature.ToString());
            var group = rec.container as GroupRecord;
            Assert.IsNotNull(group, "new record should live in a group");
            Assert.AreSame(plugin, group.container);
            Assert.IsTrue(group.elements.Contains(rec),
                "group should contain the new record");
        }

        [Test]
        public void TestAddRecordToGroupDirectly() {
            var group = plugin.AddElement("ARMO") as GroupRecord;
            Assert.IsNotNull(group);
            var rec = group.AddElement("ARMO") as MainRecord;
            Assert.IsNotNull(rec, "AddElement(group, sig) should return a record");
            Assert.AreSame(group, rec.container);
            Assert.IsTrue(group.elements.Contains(rec),
                "group should contain the new record");
        }
    }
}
