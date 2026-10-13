using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.defs {
    [TestFixture]
    public class JsonContractGateTests {
        private static IEnumerable<string> GameAbbrevs() {
            return new[] {
                "TES4", "FO3", "FNV", "TES5", "FO4", "SSE", "SF"
            };
        }

        private static JObject LoadDefs(string abbrev) {
            var zipPath = Path.Join(
                Environment.CurrentDirectory, "definitions.zip"
            );
            using var zip = ZipFile.OpenRead(zipPath);
            var entry = zip.GetEntry(abbrev + ".json");
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            return (JObject)JObject.Parse(reader.ReadToEnd())["defs"];
        }

        private static void Collect(
            JToken node,
            HashSet<string> typeIds,
            HashSet<string> deciders,
            HashSet<int> groupTypes
        ) {
            if (node is JObject obj) {
                if (obj["type"] is JValue typeValue &&
                    typeValue.Value is string typeId &&
                    typeId != "record" && typeId != "group")
                    typeIds.Add(typeId);
                if (obj["decider"] is JValue deciderValue &&
                    deciderValue.Value is string decider)
                    deciders.Add(decider);
                if (obj["groupType"] is JValue groupValue &&
                    groupValue.Value != null)
                    groupTypes.Add(Convert.ToInt32(groupValue.Value));
                foreach (var property in obj.Properties())
                    Collect(
                        property.Value, typeIds, deciders, groupTypes
                    );
            } else if (node is JArray array) {
                foreach (var child in array)
                    Collect(child, typeIds, deciders, groupTypes);
            }
        }

        // Type ids referenced by shipped JSON defs that have no C#
        // implementation yet. Each entry documents why it is deferred;
        // AllowlistEntriesAreStillUnregistered fails as soon as an id
        // becomes registered so it must then be removed here.
        private static readonly Dictionary<string, HashSet<string>>
            AllowlistedTypeIds = new Dictionary<string, HashSet<string>> {
            ["TES4"] = new HashSet<string> {
                // 4-char codes (EFID/EFIT/SCIT), xEdit display formats.
                "Char4Format",
                // CTDA Param2 union Variable-name branch display.
                "CTDAParam2VariableNameFormat",
                // IDLE ANAM display.
                "IdleAnamFormat",
                // xEdit wbMul display scaling (SOUN attenuation).
                "multiply",
                // REGN entry display scaling.
                "ScaledInt4Format",
                // WTHR DATA Classification display.
                "WthrDataClassificationFormat",
            },
            ["FO3"] = new HashSet<string> {
                "CTDAParam2VariableNameFormat",
                "IdleAnamFormat",
                "multiply",
                // NAVM triangle edge display.
                "NVTREdgeFormat",
                // PERK DATA function/EPFT display.
                "PerkDATAFunctionFormat",
                "PerkEPFTFormat",
                "PRKCFormat",
                "ScaledInt4Format",
                "WthrDataClassificationFormat",
            },
            ["FNV"] = new HashSet<string> {
                // ALOC/SOUN time display.
                "AlocTimeFormat",
                "CTDAParam2VariableNameFormat",
                // Param2 union branch-45 objective display.
                "CTDAParam2QuestObjectiveFormat",
                "IdleAnamFormat",
                "multiply",
                "NVTREdgeFormat",
                "PerkDATAFunctionFormat",
                "PerkEPFTFormat",
                "PRKCFormat",
                "ScaledInt4Format",
                "WthrDataClassificationFormat",
            },
            ["FO4"] = new HashSet<string> {
                // CLFM color display.
                "CLFMColorFormat",
                // NPC_ face-morph display.
                "FaceMorphFormat",
                // RACE morph-index display (hex width unverified).
                "IntToHexStrFormat",
                // NPC_ morph value display.
                "MorphValueFormat",
                // Recursive struct member type (known nested-struct gap).
                "recursive",
            },
        };

        [Test, TestCaseSource(nameof(GameAbbrevs))]
        public void AllTypeIdsAreRegistered(string abbrev) {
            var session = RecordDefCoverageTests.GetSession(abbrev);
            var manager = session.definitionManager;
            var typeIds = new HashSet<string>();
            Collect(
                LoadDefs(abbrev), typeIds,
                new HashSet<string>(), new HashSet<int>()
            );
            var allowlisted = AllowlistedTypeIds.TryGetValue(
                abbrev, out var allowed
            ) ? allowed : new HashSet<string>();
            var missing = typeIds
                .Where(t => !manager.IsDefTypeRegistered(t) &&
                    !allowlisted.Contains(t))
                .OrderBy(t => t)
                .ToList();
            Assert.IsEmpty(
                missing,
                $"[{abbrev}] {missing.Count} unregistered type ids: " +
                    string.Join(", ", missing.Take(100))
            );
        }

        [Test, TestCaseSource(nameof(GameAbbrevs))]
        public void AllowlistEntriesAreStillUnregistered(string abbrev) {
            var manager = RecordDefCoverageTests
                .GetSession(abbrev).definitionManager;
            var implemented = AllowlistedTypeIds.TryGetValue(
                abbrev, out var allowed
            ) ? allowed.Where(t => manager.IsDefTypeRegistered(t))
                .OrderBy(t => t).ToList() : new List<string>();
            Assert.IsEmpty(
                implemented,
                $"[{abbrev}] implemented but still allowlisted: " +
                    string.Join(", ", implemented)
            );
        }

        [Test, TestCaseSource(nameof(GameAbbrevs))]
        public void AllDecidersAreRegistered(string abbrev) {
            var session = RecordDefCoverageTests.GetSession(abbrev);
            var manager = session.definitionManager;
            var deciders = new HashSet<string>();
            Collect(
                LoadDefs(abbrev), new HashSet<string>(),
                deciders, new HashSet<int>()
            );
            var missing = deciders
                .Where(d => !manager.IsDeciderRegistered(d))
                .OrderBy(d => d)
                .ToList();
            Assert.IsEmpty(
                missing,
                $"[{abbrev}] {missing.Count} unregistered deciders: " +
                    string.Join(", ", missing.Take(100))
            );
        }

        [Test, TestCaseSource(nameof(GameAbbrevs))]
        public void AllGroupTypesAreRegistered(string abbrev) {
            var session = RecordDefCoverageTests.GetSession(abbrev);
            var groupManager = session.definitionManager.groupManager;
            var groupTypes = new HashSet<int>();
            Collect(
                LoadDefs(abbrev), new HashSet<string>(),
                new HashSet<string>(), groupTypes
            );
            var missing = groupTypes
                .Where(g => !groupManager.IsGroupTypeRegistered(g))
                .OrderBy(g => g)
                .ToList();
            Assert.IsEmpty(
                missing,
                $"[{abbrev}] {missing.Count} unregistered group types: " +
                    string.Join(", ", missing.Take(100))
            );
        }
    }
}
