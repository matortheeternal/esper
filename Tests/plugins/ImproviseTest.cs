using esper;
using esper.data;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Tests.plugins {
    public class SubrecordStat {
        public Signature sig;
        public int count;
        public Dictionary<uint, int> lengths;

        public SubrecordStat(Signature sig) {
            this.sig = sig;
            lengths = new Dictionary<uint, int>();
        }

        public void AddSubrecord(Subrecord subrecord) {
            count++;
            if (lengths.ContainsKey(subrecord.dataSize)) {
                lengths[subrecord.dataSize]++;
            } else {
                lengths.Add(subrecord.dataSize, 1);
            }
        }
    }

    public class RecordStat {
        public Signature sig;
        public int count;
        public bool processSubrecords;
        public Dictionary<Signature, SubrecordStat> subrecordStats;

        public RecordStat(Signature sig, bool processSubrecords = false) {
            this.sig = sig;
            this.processSubrecords = processSubrecords;
            if (!processSubrecords) return;
            subrecordStats = new Dictionary<Signature, SubrecordStat>();
        }

        public void AddRecord(MainRecord rec) {
            count++;
            if (!processSubrecords) return;
            foreach (var subrecord in rec.unexpectedSubrecords) {
                var subrecordSig = subrecord.signature;
                var subrecordStat = subrecordStats[subrecordSig];
                if (subrecordStat == null) {
                    subrecordStat = new SubrecordStat(subrecordSig);
                    subrecordStats.Add(subrecordSig, subrecordStat);
                }
                subrecordStat.AddSubrecord(subrecord);
            }
        }
    }

    public class ImproviseTest {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        private void LoadDumpEsm() {
            var pluginPath = TestHelpers.FixturePath("Constellation.esm");
            Assert.IsTrue(File.Exists(pluginPath),
                "See README.md for test fixture installation instructions."
            );
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions {
                improvise = true
            });
            LoadDumpEsm();
        }

        [Test]
        public void TestLoad() {
            Assert.IsNotNull(plugin);
        }

        [Test]
        public void TestSubrecordStats() {
            var manager = plugin as IRecordManager;

            /*var recordStats = new Dictionary<Signature, RecordStat>();
            foreach (var record in manager.records) {
                var recordSig = record.signature;
                var recordStat = recordStats[recordSig];
                if (recordStat == null) {
                    recordStat = new RecordStat(recordSig);
                    recordStats.Add(recordSig, recordStat);
                }
                recordStat.AddRecord(record);
            }*/

        }
    }
}
