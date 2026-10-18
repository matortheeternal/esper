using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using esper;
using esper.defs;
using esper.elements;
using esper.io;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    [TestFixture]
    public class TmpParityProbe {
        [Test]
        public void Probe() {
            var session = new Session(Games.SSE, new SessionOptions());
            var plugin = session.pluginManager.LoadPlugin(TestHelpers.FixturePath("Skyrim.esm"));
            var sigs = new HashSet<string>();
            foreach (var rec in plugin.records) {
                sigs.Add(rec.signature.ToString());
                rec._internalElements = null;
                rec._unexpectedSubrecords = null;
            }
            var samples = new Dictionary<string, List<MainRecord>>();
            foreach (var rec in plugin.records) {
                var sig = rec.signature.ToString();
                if (!samples.TryGetValue(sig, out var list)) {
                    list = new List<MainRecord>();
                    samples[sig] = list;
                }
                if (list.Count < 3) {
                    list.Add(rec);
                } else {
                    rec._internalElements = null;
                    rec._unexpectedSubrecords = null;
                }
                if (samples.Count == sigs.Count && samples.Values.All(l => l.Count >= 3)) break;
            }
            foreach (var kv in samples.OrderBy(kv => kv.Key)) {
                var sig = kv.Key;
                foreach (var rec in kv.Value) {
                    try {
                        var els = rec.elements;
                        try {
                            var unexp = rec.unexpectedSubrecords;
                            var mrDef = (MainRecordDef)rec.def;
                            var tmp = Path.GetTempFileName();
                            try {
                                var output = new PluginFileOutput(tmp, plugin);
                                mrDef.WriteElementsTo(rec, output);
                                output.writer.Close();
                                var written = File.ReadAllBytes(tmp);
                                var content = written.Skip(24).ToArray();
                                string verdict;
                                if (rec.compressed) {
                                    verdict = "COMPRESSED-SKIP";
                                } else {
                                    byte[] raw;
                                    using (var fs = new FileStream(TestHelpers.FixturePath("Skyrim.esm"), FileMode.Open, FileAccess.Read, FileShare.Read)) {
                                        fs.Seek(rec.bodyOffset, SeekOrigin.Begin);
                                        raw = new byte[rec.header.dataSize];
                                        int got = 0;
                                        while (got < raw.Length) {
                                            int n = fs.Read(raw, got, raw.Length - got);
                                            if (n == 0) break;
                                            got += n;
                                        }
                                    }
                                    if (raw.Length != content.Length) {
                                        verdict = $"LEN-DIFF raw={raw.Length} written={content.Length}";
                                    } else {
                                        int diff = -1;
                                        for (int i = 0; i < raw.Length; i++) {
                                            if (raw[i] != content[i]) { diff = i; break; }
                                        }
                                        verdict = diff < 0 ? "MATCH" : $"MISMATCH@{diff}";
                                    }
                                }
                                File.AppendAllText("/tmp/parity.txt", $"PARITY {sig}:{rec.formId.fileFormId:X8} {verdict} els={els.Count} unexp={unexp.Count} bytes={content.Length}\n");
                                if (sig == "STAT" && rec.formId.fileFormId == 0x00000001) File.WriteAllBytes("/tmp/stat_written.bin", content);
                            } finally {
                                File.Delete(tmp);
                            }
                        } catch (Exception ex) {
                            File.AppendAllText("/tmp/parity.txt", $"PARITY {sig}:{rec.formId.fileFormId:X8} ERROR {ex.GetType().Name} {ex.Message.Split('\n')[0]}\n");
                        }
                    } finally {
                        rec._internalElements = null;
                        rec._unexpectedSubrecords = null;
                    }
                }
            }
        }
    }
}
