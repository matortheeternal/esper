using esper;
using esper.plugins;
using esper.setup;
using esper.resolution;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Tests.plugins {
    public class SkyrimEsmTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public ResourceManager resourceManager;
        public PluginFile plugin;
        public Stopwatch watch = new Stopwatch();
        public Stopwatch watch2 = new Stopwatch();

        private List<string> GetStringFiles() {
            var skyrimStringsPath = TestHelpers.FixturePath("skyrim_strings");
            var stringFiles = Directory.GetFiles(
                skyrimStringsPath, "skyrim_english.*"
            ).ToList();
            Assert.IsTrue(stringFiles.Count == 3,
                "See README.md for test fixture installation instructions."
            );
            return stringFiles;
        }

        private void LoadSkyrimEsm() {
            var pluginPath = TestHelpers.FixturePath("Skyrim.esm");
            Assert.IsTrue(File.Exists(pluginPath),
                "See README.md for test fixture installation instructions."
            );
            watch.Start();
            plugin = pluginManager.LoadPlugin(pluginPath);
            watch.Stop();
        }

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions());
            resourceManager = new ResourceManager(session, true);
            LoadSkyrimEsm();
            resourceManager.LoadPluginStrings(plugin, GetStringFiles());
        }

        [Test]
        public void TestLoadTime() {
            Assert.IsNotNull(plugin);
            float ms = watch.ElapsedMilliseconds;
            Console.WriteLine($"Loaded Skyrim.esm in {ms}ms");
            Assert.IsTrue(
                ms < 1500,
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

        private void TestValue(UInt32 formId, string path, string expected) {
            var rec = plugin.GetRecordByFormId(formId);
            Assert.IsNotNull(rec);
            Assert.AreEqual(expected, rec.GetValue(path));
        }

        [Test]
        public void TestLocalizedStrings() {
            TestValue(0x012E46, "FULL", "Iron Gauntlets");
            TestValue(0x01360E, "FULL", "That was from Brynjolf. Get the message?");
            var bookText = "Client - <Client Name Goes Here>\r\n" +
                           "Location - <Dungeon Name Goes Here>";
            TestValue(0x03DD30, "DESC", bookText);
        }

        [Test]
        public void TestWeirdRecords() {
            TestValue(0x25, @"XCLL\Ambient Colors\Specular", null);
            TestValue(0x1B44B, "XNAM", "");
            TestValue(0x2BCD7, "XCIM", "{Skyrim.esm:0A2687}");
            TestValue(0xE15, @"NVNM\NavMeshGrid Divisor", "12");
            TestValue(0x15CA4, "INAM", "1");
            TestValue(0x3C, "EDID", "Tamriel");
        }

        [Test]
        public void TestReferencedBy() {
            plugin.GetElement("CONT").BuildRefBy();
            var rec = plugin.GetRecordByFormId(0xF);
            Assert.AreEqual(
                rec.referencedBy.Count, 16, 
                "Gold should be referenced by 16 CONT records"
            );
        }

        [Test]
        public void TestContainedIn() {
            var rec = plugin.GetRecordByFormId(0x23630);
            Assert.AreEqual(
                "Tamriel", rec.GetValue(@"@Cell\@Worldspace\EDID"),
                "Containing worldspace should be Tamriel."
            );
        }
    }
}
