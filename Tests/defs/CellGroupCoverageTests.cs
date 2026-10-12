using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using esper;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using Tests;
using Tests.defs;

namespace Tests.defs {
    [TestFixture]
    public class CellGroupCoverageTests {
        private static PluginFile LoadSkyrim(Session session) {
            var path = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(path))
                Assert.Ignore("Skyrim.esm missing.");
            return session.pluginManager.LoadPlugin(path);
        }

        private static List<GroupRecord> AncestorGroups(
            MainRecord record
        ) {
            var groups = new List<GroupRecord>();
            Element current = record;
            while (current.container is GroupRecord group) {
                groups.Add(group);
                current = group;
            }
            return groups;
        }

        private static List<GroupRecord> CellChain(
            PluginFile plugin, string wantBlock
        ) {
            foreach (var record in plugin.records) {
                if (record.signature.ToString() != "CELL") continue;
                var groups = AncestorGroups(record);
                if (groups.Count < 3) continue;
                if (groups.Any(g =>
                    g.groupDef.GetType().Name == wantBlock))
                    return groups;
            }
            return null;
        }

        private static int RoundtripBlocks(List<GroupRecord> groups) {
            int roundtrips = 0;
            foreach (var group in groups) {
                var def = group.groupDef;
                if (!def.GetType().Name.Contains("Block")) continue;
                string name = group.name;
                Assert.IsNotNull(name);
                if (!def.NameMatches(name)) continue;
                byte[] parsed = def.ParseLabel(group, name);
                Assert.IsNotNull(parsed);
                dynamic coords = def.ConvertLabel(group, parsed);
                if (def.GetType().Name.StartsWith("ExteriorCell")) {
                    Assert.AreEqual(
                        BitConverter.ToInt16(parsed, 0),
                        (short)coords[0]
                    );
                    Assert.AreEqual(
                        BitConverter.ToInt16(parsed, 2),
                        (short)coords[1]
                    );
                } else {
                    Assert.AreEqual(
                        BitConverter.ToInt32(parsed, 0), (int)coords
                    );
                }
                roundtrips++;
            }
            return roundtrips;
        }

        [Test]
        public void CellBlockGroupsRoundTripLabels() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var groups = CellChain(plugin, "InteriorCellBlockDef");
            if (groups == null)
                Assert.Ignore("No interior CELL block chain.");
            var names = groups
                .Select(g => g.groupDef.GetType().Name).ToHashSet();
            Assert.IsTrue(names.Contains("InteriorCellBlockDef"));
            Assert.Greater(RoundtripBlocks(groups), 0);
        }

        [Test]
        public void CellChildGroupsHaveReadableNames() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var groups = CellChain(plugin, "InteriorCellBlockDef");
            if (groups == null)
                Assert.Ignore("No interior CELL block chain.");
            Assert.IsNotNull(groups[0].name);
        }

        [Test]
        public void WorldspaceBlockGroupsRoundTrip() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var groups = CellChain(plugin, "ExteriorCellBlockDef");
            if (groups == null)
                Assert.Ignore("No exterior CELL block chain.");
            var names = groups
                .Select(g => g.groupDef.GetType().Name).ToHashSet();
            Assert.IsTrue(names.Contains("ExteriorCellBlockDef"));
            Assert.Greater(RoundtripBlocks(groups), 0);
        }
    }
}
