using esper;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests.resolution {
    public class ResolutionInterfaceTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        internal static PluginFile plugin;
        internal static PluginFile dragonborn;
        internal static PluginFile plugin2;
        internal static Element group;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            var pluginPath = TestHelpers.FixturePath("AllRecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
            group = plugin.elements.First(e => {
                return e is GroupRecord g && g.isTopGroup
                    && g.groupDef.signature.ToString() == "ACTI";
            });
            pluginPath = TestHelpers.FixturePath("Dragonborn.esm");
            dragonborn = pluginManager.LoadPlugin(pluginPath);
            pluginPath = TestHelpers.FixturePath("ARecords.esp");
            plugin2 = pluginManager.LoadPlugin(pluginPath);
        }

        [Test]
        public void TestResolution_Container() {
            var root = plugin.ResolveElement("..");
            Assert.AreEqual(root, session.root);
        }

        [Test]
        public void TestResolution_Parent() {
            var rec = group.elements[0];
            var obnd = rec.elements[2];
            var x1 = obnd.elements[0];
            Assert.AreEqual(obnd, x1.ResolveElement("^Subrecord"));
            Assert.AreEqual(rec, x1.ResolveElement("^Record"));
            Assert.AreEqual(group, x1.ResolveElement("^Group"));
            Assert.AreEqual(plugin, x1.ResolveElement("^File"));
            Assert.AreEqual(null, x1.ResolveElement("^Root"));
        }

        [Test]
        public void TestResolution_FileFormId() {
            var rec = group.elements[0];
            Assert.AreEqual(rec, plugin.ResolveElement("0xF55"));
            Assert.AreEqual(null, plugin.ResolveElement("&0"));
            Assert.AreEqual("Tamriel", dragonborn.GetValue("0x3C\\EDID"));
            Assert.AreEqual("DLC2SolstheimWorld", dragonborn.GetValue("&02000800\\EDID"));
        }

        [Test]
        public void TestResolution_Reference() {
            var rec = group.elements[0];
            Assert.AreEqual(null, rec.GetElement("Record Header\\@FormID"));
            var rec2 = plugin2.GetElement("0x800");
            Assert.AreEqual(rec2, rec2.GetElement("Record Header\\@FormID"));
            Assert.AreEqual(
                "DLC2ArmorStalhrimLightHelmet", 
                dragonborn.GetValue("&020290FF\\@TNAM\\EDID")
            );
            Assert.AreEqual(null, plugin2.GetElement("0x80B\\@YNAM"));
        }

        [Test]
        public void TestResolution_Index() {
            Assert.AreEqual(plugin.header, plugin.ResolveElement("[0]"));
            Assert.AreEqual(plugin.elements[1], plugin.ResolveElement("[1]"));
            Assert.AreEqual(null, plugin.ResolveElement("[-1]"));
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                plugin.ResolveElement("[999]");
            });
        }

        [Test]
        public void TestResolution_First() {
            var rec = group.elements[0];
            Assert.AreEqual("defaultBlankTrigger", rec.GetValue("[FULL|EDID]"));
            Assert.AreEqual("Test", 
                plugin2.GetValue("[WRLD|WEAP|ARMO]\\[0]\\[FULL|EDID]")
            );
        }

        [Test]
        public void TestResolution_Signature() {
            Assert.AreEqual(group, plugin.ResolveElement("ACTI"));
            var rec = group.elements[0];
            Assert.AreEqual(rec, group.ResolveElement("ACTI"));
            Assert.AreEqual(rec.elements[2], rec.ResolveElement("OBND"));
        }

        [Test]
        public void TestResolution_Name() {
            Assert.AreEqual(dragonborn, session.root.ResolveElement("Dragonborn.esm"));
            Assert.AreEqual(group, plugin.ResolveElement("Activator"));
            var rec = group.elements[0];
            Assert.AreEqual(rec, group.ResolveElement("defaultBlankTrigger"));
        }
    }
}
