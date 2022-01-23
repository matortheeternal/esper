using esper;
using esper.plugins;
using esper.setup;
using esper.resolution;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tests.plugins {
    public class SkyrimEsmTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public ResourceManager resourceManager;
        public string pluginPath;
        public PluginFile plugin;
        public Stopwatch watch = new Stopwatch();
        public Stopwatch watch2 = new Stopwatch();

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions());
            resourceManager = new ResourceManager(session, true);
            pluginPath = TestHelpers.FixturePath("Skyrim.esm");
            Assert.IsTrue(File.Exists(pluginPath),
                "See README.md for test fixture installation instructions."
            );
            watch.Start();
            plugin = pluginManager.LoadPlugin(pluginPath);
            watch.Stop();
            var skyrimStringsPath = TestHelpers.FixturePath("skyrim_strings");
            var stringFiles = Directory.GetFiles(
                skyrimStringsPath, "skyrim_english.*"
            ).ToList();
            Assert.IsTrue(stringFiles.Count == 3,
                "See README.md for test fixture installation instructions."
            );
            resourceManager.LoadPluginStrings(plugin, stringFiles);
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
            var records = (plugin as IRecordManager).records;
            for (int i = 0; i < records.Count; i++) {
                rec = records[i];
                var r2 = plugin.GetRecordByFormId(rec.fileFormId);
                Assert.IsNotNull(
                    r2,
                    $"Failed to find record {rec.fileFormId}, index: {i}"
                );
            }
        }

        [Test]
        public void BenchmarkRecordMap() {
            watch.Reset();
            var random = new Random();
            var records = (plugin as IRecordManager).records;
            var n = 0;
            watch2.Start();
            while (watch2.ElapsedMilliseconds < 1000) {
                n++;
                var index = random.Next(records.Count);
                var rec = records[index];
                var fid = rec.fileFormId;
                watch.Start();
                var rec2 = plugin.GetRecordByFormId(fid);
                watch.Stop();
                Assert.IsNotNull(rec2);
            }
            watch2.Stop();
            Console.WriteLine($"GetRecordByFormId called {n} times");
            var avgTime = watch.Elapsed.TotalMilliseconds / n;
            Console.WriteLine($"Average execution time: {avgTime:N5}ms");
            Assert.IsTrue(
                avgTime < 1,
                "Expected to find form IDs in less than 1ms."
            );
        }

        private void TestText(UInt32 formId, string path, string expectedText) {
            var rec = plugin.GetRecordByFormId(formId);
            Assert.IsNotNull(rec);
            Assert.AreEqual(expectedText, rec.GetValue(path));
        }

        [Test]
        public void TestLocalizedStrings() {
            TestText(0x012E46, "FULL", "Iron Gauntlets");
            TestText(0x01360E, "FULL",
                "That was from Brynjolf. Get the message?");
            TestText(0x03DD30, "DESC",
                "Client - <Client Name Goes Here>\r\nLocation - <Dungeon Name Goes Here>");
        }
    }
}
