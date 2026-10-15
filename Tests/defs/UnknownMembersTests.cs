using esper;
using esper.defs;
using esper.elements;
using esper.io;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Tests.defs {
    [TestFixture]
    public class UnknownMembersTests {
        private static byte[] ReadBodyBytes(string path, MainRecord record) {
            using (var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite
            )) {
                stream.Seek(record.bodyOffset, SeekOrigin.Begin);
                var raw = new byte[record.header.dataSize];
                var got = 0;
                while (got < raw.Length) {
                    var n = stream.Read(raw, got, raw.Length - got);
                    if (n == 0) break;
                    got += n;
                }
                return raw;
            }
        }

        private static byte[] WriteRecordBody(PluginFile plugin, MainRecord record) {
            var mrDef = (MainRecordDef)record.def;
            var tmp = Path.GetTempFileName();
            try {
                var output = new PluginFileOutput(tmp, plugin);
                mrDef.WriteElementsTo(record, output);
                output.writer.Close();
                return File.ReadAllBytes(tmp).Skip(24).ToArray();
            } finally {
                File.Delete(tmp);
            }
        }

        [Test]
        public void UnknownSubrecordsDecodeAsUnknownBytesAndRoundTrip() {
            var fixturePath = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(fixturePath))
                Assert.Ignore($"Local fixture not found: {fixturePath}");

            var session = new Session(Games.SSE, new SessionOptions());
            var plugin = session.pluginManager.LoadPlugin(fixturePath);
            var warningsBefore = session.logger.messages.Count(
                m => m.messageType == LogMessageType.Warning);

            MainRecord proven = null;
            Element provenUnknown = null;
            string lastVerdict = "no candidate with unknown subrecords found";
            foreach (var record in plugin.records) {
                if (proven != null) break;
                if (record.compressed) continue;
                Element unknown = null;
                try {
                    unknown = record.elements.FirstOrDefault(el =>
                        el.name == "Unknown" && el.def is BytesDef &&
                        session.definitionManager.GetDefinitionGaps().Any(
                            gap => gap.Contains(el.def.signature.ToString())));
                    if (unknown == null) continue;
                    var written = WriteRecordBody(plugin, record);
                    var raw = ReadBodyBytes(fixturePath, record);
                    if (raw.SequenceEqual(written)) {
                        proven = record;
                        provenUnknown = unknown;
                        break;
                    }
                    lastVerdict =
                        $"{record.signature}:{record.fileFormId:X8} " +
                        $"raw={raw.Length} written={written.Length}";
                } catch {
                } finally {
                    if (proven == null) {
                        record._internalElements = null;
                        record._unexpectedSubrecords = null;
                    }
                }
            }
            Assert.IsNotNull(proven, "No round-tripping record: " + lastVerdict);
            try {
                Assert.IsInstanceOf<BytesDef>(provenUnknown.def);
                Assert.IsEmpty(proven.unexpectedSubrecords);

                var unknownSig = provenUnknown.def.signature.ToString();
                Assert.IsTrue(
                    session.definitionManager.GetDefinitionGaps().Any(
                        gap => gap.Contains(unknownSig)),
                    $"No definition gap reported for {unknownSig}.");
                var warningsAfter = session.logger.messages.Count(
                    m => m.messageType == LogMessageType.Warning);
                Assert.AreEqual(warningsBefore, warningsAfter);
            } finally {
                proven._internalElements = null;
                proven._unexpectedSubrecords = null;
            }
        }
    }
}
