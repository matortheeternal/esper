using esper;
using esper.data;
using esper.defs;
using esper.setup;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Tests.defs {
    // Verifies that every record definition shipped in definitions.zip
    // resolves to a working def object for its game. Session construction
    // eagerly builds ALL defs (DefinitionManager.BuildDefs), so these tests
    // fail on any malformed def JSON or unimplemented def type that the
    // library silently nulled out.
    public class RecordDefCoverageTests {
        internal static readonly Dictionary<string, Game> games =
            new Dictionary<string, Game> {
                { "TES4", Games.TES4 },
                { "FO3", Games.FO3 },
                { "FNV", Games.FNV },
                { "TES5", Games.TES5 },
                { "FO4", Games.FO4 },
                { "SSE", Games.SSE },
                { "SF", Games.SF },
            };

        internal static readonly Dictionary<string, Session> sessions =
            new Dictionary<string, Session>();

        internal static Session GetSession(string abbrev) {
            if (!sessions.ContainsKey(abbrev))
                sessions[abbrev] = new Session(
                    games[abbrev], new SessionOptions()
                );
            return sessions[abbrev];
        }

        internal static List<string> GetRecordSigs(string abbrev) {
            var zipPath = Path.Join(Environment.CurrentDirectory, "definitions.zip");
            using (var archive = ZipFile.OpenRead(zipPath)) {
                var entry = archive.GetEntry(abbrev + ".json");
                Assert.IsNotNull(entry, $"No {abbrev}.json in definitions.zip");
                using (var reader = new StreamReader(entry.Open())) {
                    var json = JObject.Load(new JsonTextReader(reader));
                    var defs = json.Value<JObject>("defs");
                    return defs.Properties()
                        .Where(p =>
                            p.Value.Value<string>("type") == "record" &&
                            p.Value.Value<string>("signature") != null
                        )
                        .Select(p => p.Value.Value<string>("signature"))
                        .OrderBy(k => k)
                        .ToList();
                }
            }
        }

        public static IEnumerable<TestCaseData> GameCases() {
            foreach (var abbrev in games.Keys)
                yield return new TestCaseData(abbrev).SetName($"{{{abbrev}}}");
        }

        public static IEnumerable<TestCaseData> RecordDefCases() {
            foreach (var abbrev in games.Keys)
                foreach (var sig in GetRecordSigs(abbrev))
                    yield return new TestCaseData(abbrev, sig)
                        .SetName($"{{{abbrev}:{sig}}}");
        }

        [Test, TestCaseSource("GameCases")]
        public void SessionBuildsAllDefinitions(string abbrev) {
            var session = GetSession(abbrev);
            Assert.IsNotNull(session.definitionManager);
        }

        [Test, TestCaseSource("RecordDefCases")]
        public void RecordDefResolves(string abbrev, string sig) {
            var session = GetSession(abbrev);
            var def = session.definitionManager.GetRecordDef(
                Signature.FromString(sig)
            );
            Assert.IsNotNull(def, $"No record def resolved for {sig}");
            Assert.AreEqual(sig, def.signature.ToString());
            Assert.IsNotNull(def.name);
            Assert.AreNotEqual("", def.name);
        }

        [Test, TestCaseSource("RecordDefCases")]
        public void RecordDefStructureValid(string abbrev, string sig) {
            var session = GetSession(abbrev);
            var def = (MainRecordDef)session.definitionManager.GetRecordDef(
                Signature.FromString(sig)
            );
            Assert.IsNotNull(def.headerDef, $"{sig} has no header def");
            Assert.IsNotNull(def.memberDefs, $"{sig} has no member defs");
            // member subrecord signatures aggregated from the def tree
            var sigs = def.GetSignatures();
            Assert.IsNotNull(sigs, $"{sig} GetSignatures() is null");
            Assert.IsNotEmpty(sigs, $"{sig} def has no member signatures");
        }

        [Test, TestCaseSource("GameCases")]
        public void RecordDefCountMatchesJson(string abbrev) {
            var session = GetSession(abbrev);
            var expected = GetRecordSigs(abbrev).Count;
            var resolved = GetRecordSigs(abbrev).Count(sig => {
                return session.definitionManager.GetRecordDef(
                    Signature.FromString(sig)
                ) != null;
            });
            Assert.AreEqual(expected, resolved);
        }
    }
}
