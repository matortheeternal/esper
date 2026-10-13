using esper;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.plugins {
    [TestFixture]
    public class MasterListTests {
        [Test]
        public void EmptyMasterList() {
            var masters = new MasterList(null);
            Assert.IsNull(masters.parentFile);
            Assert.AreEqual(0, masters.Count);
            Assert.AreEqual(0, masters.files.Count);
            Assert.AreEqual(0, masters.filenames.Count);
            Assert.IsNull(masters.OrdinalToFile(0));
            Assert.AreEqual(0, masters.FileToOrdinal(null));
            masters.Add(null);
            Assert.AreEqual(1, masters.Count);
            Assert.IsTrue(masters.Contains(null));
            Assert.AreEqual(0, masters.FileToOrdinal(null));
            masters.Remove(null);
            Assert.AreEqual(0, masters.Count);
            Assert.IsFalse(masters.Contains(null));
        }

        [Test]
        public void RealMasterList() {
            var path = TestHelpers.FixturePath("AllRecords.esp");
            if (!File.Exists(path)) Assert.Ignore("AllRecords.esp missing.");
            var session = new Session(Games.SSE, new SessionOptions());
            var plugin = session.pluginManager.LoadPlugin(path);
            var masters = plugin.masters;
            if (masters.Count == 0) Assert.Ignore("No masters listed.");
            Assert.AreEqual(masters.Count, masters.files.Count);
            Assert.AreEqual(masters.Count, masters.filenames.Count);
            Assert.AreEqual(
                masters.files[0].filename, masters.filenames[0]
            );
            Assert.AreEqual(masters.files[0], masters.OrdinalToFile(0));
            Assert.AreEqual(0, masters.FileToOrdinal(masters.files[0]));
            Assert.IsTrue(masters.Contains(masters.files[0]));
            Assert.AreEqual(
                masters.parentFile,
                masters.OrdinalToFile((byte)masters.Count)
            );
        }

        [Test]
        public void ReadOnlyMasterListThrows() {
            var masters = new ReadOnlyMasterList(
                null, new List<PluginFile>()
            );
            Assert.AreEqual(0, masters.Count);
            Assert.Throws<Exception>(() => masters.Add(null));
            Assert.Throws<Exception>(() => masters.Remove(null));
        }
    }
}
