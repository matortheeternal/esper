using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.defs {
    // Structural tests for definition wiring fixed during the coverage
    // work: FO4 quest child groups, XCRI/OMOD counter registration,
    // MemberStructDef creation, FO3 trap-record tolerance, and SF
    // resource handling without balsa support.
    public class DefinitionWiringTests {
        private static IEnumerable<ElementDef> Walk(ElementDef def) {
            yield return def;
            foreach (var child in def.childDefs ?? Enumerable.Empty<ElementDef>())
                foreach (var nested in Walk(child))
                    yield return nested;
        }

        [Test]
        public void FO4QuestTopGroupHasQuestChildren() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var qust = (session.pluginFileDef.childDefs ?? Enumerable.Empty<ElementDef>())
                .OfType<GroupDef>()
                .FirstOrDefault(g => g.signature.ToString() == "QUST");
            Assert.IsNotNull(qust, "FO4 QUST top group missing");
            var questChildren = (qust.childDefs ?? Enumerable.Empty<ElementDef>())
                .OfType<GroupDef>()
                .FirstOrDefault(g => g.groupType == 10);
            Assert.IsNotNull(questChildren, "QUST quest-children group (type 10) missing");
            var sigs = (questChildren.childDefs ?? Enumerable.Empty<ElementDef>())
                .Select(d => d.signature.ToString())
                .ToHashSet();
            Assert.IsTrue(
                sigs.IsSupersetOf(new[] { "DLBR", "DIAL", "SCEN" }),
                "Quest children lack DLBR/DIAL/SCEN: " + string.Join(",", sigs)
            );
        }

        [Test]
        public void FO4CombinedReferenceCountersAreWired() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var manager = session.definitionManager;
            var counterTypes = Walk(manager.GetRecordDef(Signature.FromString("CELL")))
                .OfType<ArrayDef>()
                .Where(a => a.counterDef != null)
                .Select(a => a.counterDef.GetType().Name)
                .ToHashSet();
            Assert.IsTrue(counterTypes.Contains("CELLCombinedMeshesCounter"));
            Assert.IsTrue(counterTypes.Contains("CELLCombinedRefsCounter"));

            var omodCounters = Walk(manager.GetRecordDef(Signature.FromString("OMOD")))
                .OfType<ArrayDef>()
                .Where(a => a.counterDef != null)
                .Select(a => a.counterDef.GetType().Name)
                .ToHashSet();
            Assert.IsTrue(omodCounters.Contains("OMODDataIncludeCounter"));
            Assert.IsTrue(omodCounters.Contains("OMODDataPropertyCounter"));
        }

        [Test]
        public void CELLCombinedRefsCounterHalvesAndDoubles() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var manager = session.definitionManager;
            var meshes = Walk(manager.GetRecordDef(Signature.FromString("CELL")))
                .OfType<ArrayDef>()
                .Select(a => a.counterDef)
                .OfType<esper.defs.FO4.CELLCombinedRefsCounter>()
                .FirstOrDefault();
            Assert.IsNotNull(meshes);
            Assert.IsTrue(meshes.canSetCount);
            Assert.AreEqual("References Count", meshes.path);
        }

        [Test]
        public void MemberStructDefCreatesMemberStructElements() {
            // FO4 ALCH with no effects previously crashed initializing its
            // required Effects member array because MemberStructDef had no
            // NewElement override (base throws NotImplementedException).
            var session = RecordDefCoverageTests.GetSession("FO4");
            var alch = session.definitionManager.GetRecordDef(
                Signature.FromString("ALCH")
            );
            var effects = Walk(alch)
                .OfType<MemberArrayDef>()
                .FirstOrDefault(a => a.name == "Effects");
            Assert.IsNotNull(effects, "ALCH Effects member array missing");
            Assert.IsInstanceOf<MemberStructDef>(effects.memberDef);
            Assert.IsInstanceOf<MemberStructElement>(
                effects.memberDef.NewElement(null)
            );
        }

        [Test]
        public void FO3ToleratesTrapRecordRefs() {
            // Upstream esp.json lists FO3 trap records (PFLA etc.) in cell
            // child groups without shipping record defs for them. Sessions
            // must still build; only pure id-refs are filtered.
            var traps = new[] { "PFLA", "PBAR", "PARW", "PCON", "PHZD" };
            foreach (var abbrev in new[] { "FO3", "FNV" }) {
                var session = RecordDefCoverageTests.GetSession(abbrev);
                Assert.IsFalse(session.definitionManager.HasDefEntry("PFLA"));
                // The dangling pure id-refs must be gone from the built
                // tree, while real groups (e.g. CELL) survive filtering.
                var built = Walk(session.pluginFileDef).ToList();
                Assert.IsTrue(built.OfType<GroupDef>().Any(
                    g => g.signature.ToString() == "CELL"
                ));
                var recordSigs = built.OfType<MainRecordDef>()
                    .Select(d => d.signature.ToString())
                    .ToHashSet();
                Assert.IsEmpty(recordSigs.Intersect(traps));
            }
        }

        [Test]
        public void StarfieldSessionWorksWithoutBalsaResources() {
            var session = RecordDefCoverageTests.GetSession("SF");
            Assert.IsNull(session.resourceManager.assetManager);
            var ex = Assert.Throws<NotSupportedException>(() => {
                session.resourceManager.LoadData();
            });
            StringAssert.Contains("Starfield", ex.Message);
        }

        [Test]
        public void SupportedGameSessionKeepsAssetManager() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            Assert.IsNotNull(session.resourceManager.assetManager);
        }
    }
}
