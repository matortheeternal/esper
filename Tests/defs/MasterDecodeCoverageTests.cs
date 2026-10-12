using esper;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tests.defs {
    public class MasterDecodeCoverageTests {
        public static IEnumerable<TestCaseData> MasterCases() {
            yield return MasterCase(Games.TES4, "Oblivion.esm");
            yield return MasterCase(Games.SSE, "Skyrim.esm");
            yield return MasterCase(Games.SSE, "Update.esm");
            yield return MasterCase(Games.SSE, "Dragonborn.esm");
            yield return MasterCase(Games.SSE, "Vigilant.esm");
            yield return MasterCase(Games.FNV, "FalloutNV.esm");
            yield return MasterCase(Games.FO4, "Fallout4.esm");
        }

        private static TestCaseData MasterCase(Game game, string filename) {
            return new TestCaseData(game, filename)
                .SetName($"{{DECODE:{game.abbreviation}:{filename}}}");
        }

        [Test]
        [Category("MasterDecode")]
        [TestCaseSource("MasterCases")]
        public void EveryRecordDecodes(Game game, string filename) {
            if (Environment.GetEnvironmentVariable("ESPER_RUN_MASTER_TESTS") != "1")
                Assert.Ignore("Set ESPER_RUN_MASTER_TESTS=1 to run master sweeps.");

            var path = TestHelpers.FixturePath(filename);
            if (!File.Exists(path))
                Assert.Ignore($"Local fixture not found: {filename}");

            var watch = Stopwatch.StartNew();
            var session = new Session(game, new SessionOptions());
            var plugin = session.pluginManager.LoadPlugin(path);
            var recordCounts = new Dictionary<string, int>();
            var unexpectedCounts = new Dictionary<string, int>();
            var failures = new List<string>();
            var recordFilter = (Environment.GetEnvironmentVariable(
                "ESPER_MASTER_RECORD_FILTER"
            ) ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var record in plugin.records) {
                var signature = record.signature.ToString();
                var recordKey = $"{signature}:{record.fileFormId:X8}";
                if (recordFilter.Count > 0 && !recordFilter.Contains(recordKey))
                    continue;
                recordCounts.TryGetValue(signature, out var count);
                recordCounts[signature] = count + 1;
                try {
                    _ = record.elements.Count;
                    var unexpected = record.unexpectedSubrecords;
                    foreach (var subrecord in unexpected) {
                        var subrecordSignature = subrecord.signature.ToString();
                        unexpectedCounts.TryGetValue(subrecordSignature, out count);
                        unexpectedCounts[subrecordSignature] = count + 1;
                    }
                } catch (Exception exception) {
                    if (failures.Count < 25) {
                        failures.Add(
                            $"{recordKey} " +
                            $"offset=0x{record.bodyOffset:X} " +
                            $"size={record.header.dataSize} " +
                            exception
                        );
                    }
                } finally {
                    record._internalElements = null;
                    record._unexpectedSubrecords = null;
                }
            }

            watch.Stop();
            var summary = string.Join(
                ", ", recordCounts.OrderBy(p => p.Key)
                    .Select(p => $"{p.Key}={p.Value}")
            );
            TestContext.Progress.WriteLine(
                $"Decoded {plugin.records.Count} records from {filename} " +
                $"in {watch.Elapsed.TotalSeconds:N2}s: {summary}"
            );
            if (unexpectedCounts.Count > 0) {
                var unexpectedSummary = string.Join(
                    ", ", unexpectedCounts.OrderBy(p => p.Key)
                        .Select(p => $"{p.Key}={p.Value}")
                );
                TestContext.Progress.WriteLine(
                    $"Preserved unexpected subrecords: {unexpectedSummary}"
                );
            }

            Assert.IsEmpty(
                failures,
                "Record decode failures:\n" + string.Join("\n", failures)
            );
        }
    }
}
