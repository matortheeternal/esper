using esper;
using esper.data;
using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Starfield {
    public class RecordEntry {
        public int key;
        public int parent = 0;

        public RecordEntry(int key) {
            this.key = key;
        }
    }

    public class RecordInfo {
        public Signature signature;
        public List<RecordEntry> subrecordStructure;
        public Dictionary<int, int> minSubrecordCounts;
        public Dictionary<int, int> maxSubrecordCounts;

        public RecordInfo(MainRecord record) {
            signature = record.signature;
            subrecordStructure = new List<RecordEntry>();
            maxSubrecordCounts = new Dictionary<int, int>();
            minSubrecordCounts = new Dictionary<int, int>();
        }

        public void UpdateSubrecordCounts(Dictionary<int, int> counts) {
            foreach (var entry in counts) {
                if (!minSubrecordCounts.ContainsKey(entry.Key))
                    minSubrecordCounts[entry.Key] = int.MaxValue;
                if (!maxSubrecordCounts.ContainsKey(entry.Key))
                    maxSubrecordCounts[entry.Key] = 0;
                if (entry.Value < minSubrecordCounts[entry.Key])
                    minSubrecordCounts[entry.Key] = entry.Value;
                if (entry.Value > maxSubrecordCounts[entry.Key])
                    maxSubrecordCounts[entry.Key] = entry.Value;
            }
        }

        internal void UpdateParentSubrecord(List<int> seenSubrecords, int seenIndex) {
            for (int i = seenIndex + 1; i < seenSubrecords.Count - 1; i++) {
                var key = seenSubrecords[i];
                var entry = subrecordStructure.Find(entry => entry.key == key);
                if (entry.parent == 0)
                    entry.parent = seenSubrecords[seenIndex];
            }
        }

        internal void AddSubrecordEntry(int key, int? prev) {
            var entry = subrecordStructure.Find(entry => entry.key == key);
            if (entry != null) return;
            var newEntry = new RecordEntry(key);
            if (prev == null) {
                subrecordStructure.Add(newEntry);
            } else {
                var index = subrecordStructure.FindIndex(entry => entry.key == prev);
                subrecordStructure.Insert(index + 1, newEntry);
            }
        }

        public void UpdateStructure(MainRecord record) {
            int? prevSubrecord = null;
            List<int> seenSubrecords = new List<int>();
            foreach (var subrecord in record.unexpectedSubrecords) {
                var key = subrecord.signature.v;
                var seenIndex = seenSubrecords.LastIndexOf(key);
                if (seenIndex == -1) {
                    AddSubrecordEntry(key, prevSubrecord);
                    seenSubrecords.Add(key);
                } else {
                    UpdateParentSubrecord(seenSubrecords, seenIndex);
                }
                prevSubrecord = key;
            }
        }

        internal JObject GetSubrecordStats() {
            var subrecords = new JObject();
            foreach (var key in minSubrecordCounts.Keys) {
                var sig = new Signature(key);
                var min = minSubrecordCounts[key];
                var max = maxSubrecordCounts[key];
                if (max == 1) {
                    subrecords.Add(sig.ToString(), "member");
                } else if (max == min) {
                    subrecords.Add(sig.ToString(), $"array({min})");
                } else {
                    subrecords.Add(sig.ToString(), "array(*)");
                }
            }
            return subrecords;
        }

        public void Export(string outputFolder) {
            var sig = signature.ToString();
            var output = new JObject { { "signature", sig } };
            var subrecords = GetSubrecordStats();
            output.Add("subrecords", subrecords);
            var outputPath = Path.Join(outputFolder, $"{sig}.json");
            File.WriteAllText(outputPath, output.ToString());
        }
    }

    public class SubrecordInfo {
        public Signature signature;
        public Dictionary<int, int> recordEntries;
        public bool shared => recordEntries.Count > 1;

        public SubrecordInfo(int key) {
            signature = new Signature(key);
            recordEntries = new Dictionary<int, int>();
        }

        public void AddToRecordEntry(MainRecord record, int count) {
            var key = record.signature.v;
            if (!recordEntries.ContainsKey(key))
                recordEntries[key] = 0;
            recordEntries[key] += count;
        }

        public void Export(string outputFolder) {

        }
    }

    class Program {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public DefinitionManager definitionManager => session.definitionManager;
        public PluginFile plugin;
        public Dictionary<int, SubrecordInfo> subrecordInfos;
        public Dictionary<int, RecordInfo> recordInfos;

        public static string outputPath = Path.Combine(
            Environment.CurrentDirectory,
            "output"
        );

        public static string fixturesPath = Path.Combine(
            Environment.CurrentDirectory,
            "fixtures"
        );

        public static string FixturePath(string filename) {
            return Path.Combine(fixturesPath, filename);
        }

        private void LoadStarfieldEsm() {
            var pluginPath = FixturePath("Starfield.esm");
            plugin = pluginManager.LoadPlugin(pluginPath);
        }

        public void SetUp() {
            subrecordInfos = new Dictionary<int, SubrecordInfo>();
            recordInfos = new Dictionary<int, RecordInfo>();
            session = new Session(Games.SF, new SessionOptions { improvise = true });
            LoadStarfieldEsm();
            Directory.CreateDirectory(outputPath);
        }

        internal RecordInfo GetRecordInfo(MainRecord record) {
            var key = record.signature.v;
            if (!recordInfos.ContainsKey(key))
                recordInfos[key] = new RecordInfo(record);
            return recordInfos[key];
        }

        internal SubrecordInfo GetSubrecordInfo(int key) {
            if (!subrecordInfos.ContainsKey(key))
                subrecordInfos[key] = new SubrecordInfo(key);
            return subrecordInfos[key];
        }

        internal Dictionary<int, int> CountSubrecords(MainRecord record) {
            var subrecordCounts = new Dictionary<int, int>();
            foreach (var subrecord in record.unexpectedSubrecords) {
                var key = subrecord.signature.v;
                if (!subrecordCounts.ContainsKey(key))
                    subrecordCounts[key] = 0;
                subrecordCounts[key]++;
            }
            return subrecordCounts;
        }

        internal void EvaluateSubrecords() {
            var manager = plugin as IRecordManager;
            foreach (var record in manager.records) {
                var recordInfo = GetRecordInfo(record);
                var subrecordCounts = CountSubrecords(record);
                recordInfo.UpdateStructure(record);
                recordInfo.UpdateSubrecordCounts(subrecordCounts);
                foreach (var entry in subrecordCounts) {
                    var subrecordInfo = GetSubrecordInfo(entry.Key);
                    subrecordInfo.AddToRecordEntry(record, entry.Value);
                }
            }
        }

        public void ExportEvaluation() {
            foreach (var recordInfo in recordInfos.Values)
                recordInfo.Export(outputPath);
            foreach (var subrecordInfo in subrecordInfos.Values)
                subrecordInfo.Export(outputPath);
        }

        public void ExportDefinitions() {
            definitionManager.UpdateDefs();
            definitionManager.ExportDefinitions("SF-updated.json");
            definitionManager.ExportRecordSignatureList();
            Console.WriteLine($"Definitions exported successfully.");
        }

        static void Main(string[] args) {
            var watch = new Stopwatch();
            var p = new Program();
            watch.Start();
            p.SetUp();
            watch.Stop();
            Console.WriteLine($"{watch.ElapsedMilliseconds}ms spent setting up.");
            Console.WriteLine($"Evaluating subrecords.");
            //p.EvaluateSubrecords();
            //p.ExportEvaluation();
            // STEP 3: figure out which subrecords are strings
            // STEP 4: figure out which subrecords are FormIDs
            // STEP 5: figure out which subrecords are floats
            // STEP 6: figure out which subrecords are int/uint
            // STEP 9: figure out which subrecords are arrays
            // STEP 10: figure out which subrecords are structs
            Console.WriteLine($"Exporting definitions.");
            p.ExportDefinitions();
        }
    }
}
