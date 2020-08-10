using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace Tests {
    public class SkyrimEsmTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public string pluginPath;
        public PluginFile plugin;
        public Stopwatch watch = new Stopwatch();

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {
                readAllSubrecords = false
            });
            pluginPath = TestHelpers.FixturePath("Skyrim.esm");
            Assert.IsTrue(File.Exists(pluginPath),
                "Put Skyrim.esm in the fixtures directory to run these tests."
            );
            watch.Start();
            plugin = pluginManager.LoadPlugin(pluginPath);
            watch.Stop();
        }

        [Test]
        public void TestLoadTime() {
            Assert.IsNotNull(plugin);
            float seconds = watch.ElapsedMilliseconds / 1000.0f;
            Console.WriteLine($"Loaded Skyrim.esm in {seconds:N2}s");
            Assert.IsTrue(
                seconds < 1.5f,
                "Expected Skyrim.esm to load in under a 1.5 seconds."
            );
        }

        [Test]
        public void TestRecordMap() {
            var rec = plugin.GetRecordByFormId(0x012E46);
            Assert.IsNotNull(rec);
            Assert.AreEqual("ArmorIronGauntlets", rec.editorId);
            rec = plugin.GetRecordByFormId(0xF);
            Assert.IsNotNull(rec);
            Assert.AreEqual("Gold001", rec.editorId);
        }
    }
}
