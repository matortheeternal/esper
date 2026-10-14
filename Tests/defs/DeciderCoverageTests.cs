using esper;
using esper.defs.TES5;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using esper.warnings;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tests;

namespace Tests.defs {
    // Direct branch coverage for two switch-heavy TES5 deciders, driven
    // by real Skyrim.esm records. Each case decodes a genuine record and
    // invokes the decider with the same container the production read
    // path uses, asserting the exact union branch index.
    [TestFixture]
    public class DeciderCoverageTests {
        private Session session;
        private PluginFile skyrim;

        [OneTimeSetUp]
        public void SetUp() {
            session = RecordDefCoverageTests.GetSession("SSE");
            skyrim = Fixture.Master(Games.SSE, "Skyrim.esm");
        }

        private MainRecord SkyrimRec(uint formId) {
            var rec = skyrim.GetRecordByFormId(formId);
            Assert.IsNotNull(rec, $"Skyrim.esm has no record {formId:X8}");
            return rec;
        }

        private static Container Child(Container parent, string name) {
            var child = parent.elements.FirstOrDefault(e => e.name == name)
                as Container;
            Assert.IsNotNull(child, $"No element named {name}");
            return child;
        }

        private static Container MgefData(MainRecord rec) {
            return Child(Child(rec, "Magic Effect Data"), "Data");
        }

        // MGEF Archtype value -> MGEFAssocItemDecider branch, from real
        // Skyrim.esm magic effects.
        [TestCase(0x1EA6Cu, 1)] // Light
        [TestCase(0x1CE9Eu, 2)] // Bound Weapon
        [TestCase(0x1CEAAu, 3)] // Summon Creature
        [TestCase(0x2113Fu, 4)] // Guide
        [TestCase(0x16D44u, 4)] // Spawn Hazard
        [TestCase(0x3AE9Eu, 5)] // Cloak
        [TestCase(0x6E1DDu, 6)] // Werewolf
        [TestCase(0x2C56Fu, 7)] // Enhance Weapon
        [TestCase(0x17120u, 8)] // Peak Value Modifier
        [TestCase(0x12F03u, 0)] // Value Modifier (default arm)
        public void MgefAssocItemBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            var data = MgefData(rec);
            Assert.AreEqual(
                expected, new MGEFAssocItemDecider().Decide(data)
            );
        }

        [Test]
        public void MgefAssocItemMissingArchtypeReturnsZero() {
            var rec = SkyrimRec(0x12F03);
            // The record itself has no direct Archtype child, exercising
            // the null-element guard.
            Assert.AreEqual(0, new MGEFAssocItemDecider().Decide(rec));
        }

        [Test]
        public void MgefAssocItemVampireLordBranch() {
            // No Skyrim.esm MGEF uses Archtype 46 (Vampire Lord), so drive
            // the arm by retargeting a real Archtype element, then restore.
            var rec = SkyrimRec(0x12F03);
            var data = MgefData(rec);
            var arch = data.elements.First(e => e.name == "Archtype")
                as ValueElement;
            var original = arch.data;
            try {
                arch.data = 46u;
                Assert.AreEqual(6, new MGEFAssocItemDecider().Decide(data));
            } finally {
                arch.data = original;
            }
        }

        [Test]
        public void MgefAssocItemDecodesThroughProductionPath() {
            var rec = SkyrimRec(0x1EA6C);
            var assoc = MgefData(rec).elements
                .First(e => e.name == "Assoc. Item");
            Assert.AreEqual("{Skyrim.esm:03FA58}", assoc.GetValue());
        }

        private static Container RefrHeader(MainRecord rec) {
            return Child(rec, "Record Header");
        }

        // REFR base-record signature -> REFRRecordFlagsDecider branch.
        [TestCase(0x00DD1u, 1)] // placed ACTI
        [TestCase(0x00DDAu, 1)] // placed STAT
        [TestCase(0x12DEEu, 1)] // placed TREE
        [TestCase(0x12DF8u, 2)] // placed CONT
        [TestCase(0x12DFEu, 3)] // placed DOOR
        [TestCase(0x00E10u, 4)] // placed LIGH
        [TestCase(0x1389Cu, 5)] // placed MSTT
        [TestCase(0x1704Fu, 7)] // placed SCRL (equipment arm)
        [TestCase(0x00E1Au, 7)] // placed AMMO
        [TestCase(0x157C1u, 7)] // placed ARMO
        [TestCase(0x00DD9u, 7)] // placed BOOK
        [TestCase(0x00E32u, 7)] // placed INGR
        [TestCase(0x174E8u, 7)] // placed KEYM
        [TestCase(0x00E18u, 7)] // placed MISC
        [TestCase(0x15B9Cu, 7)] // placed SLGM
        [TestCase(0x00E2Fu, 7)] // placed WEAP
        [TestCase(0x00E1Du, 7)] // placed ALCH
        [TestCase(0x00DEEu, 0)] // placed SOUN (default arm)
        public void RefrRecordFlagsBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new REFRRecordFlagsDecider().Decide(RefrHeader(rec))
            );
        }

        [Test]
        public void RefrRecordFlagsAddnBranch() {
            // No REFR placed on an ADDN base was found in the sampled
            // records, so retarget a real REFR at a real ADDN base record.
            var rec = SkyrimRec(0x00DD1);
            var original = rec.GetValue("NAME");
            try {
                rec.SetValue("NAME", "{Skyrim.esm:01951F}");
                Assert.AreEqual(
                    6, new REFRRecordFlagsDecider().Decide(RefrHeader(rec))
                );
            } finally {
                rec.SetValue("NAME", original);
            }
        }

        [Test]
        public void RefrRecordFlagsUnresolvableBaseReturnsZero() {
            var rec = SkyrimRec(0x00DD1);
            var original = rec.GetValue("NAME");
            try {
                rec.SetValue("NAME", "{Skyrim.esm:FFFFFF}");
                Assert.AreEqual(
                    0, new REFRRecordFlagsDecider().Decide(RefrHeader(rec))
                );
            } finally {
                rec.SetValue("NAME", original);
            }
        }

        [Test]
        public void RefrRecordFlagsNullContainerReturnsZero() {
            Assert.AreEqual(0, new REFRRecordFlagsDecider().Decide(null));
        }

        [Test]
        public void BaseFormIdFormatStubsThrow() {
            var format = new esper.data.FormIdFormat();
            Assert.Throws<System.NotImplementedException>(
                () => format.ToString(null)
            );
            Assert.Throws<System.NotImplementedException>(
                () => format.Parse(null, null)
            );
        }

        [Test]
        public void RefrFlagsDecodeThroughProductionPath() {
            var rec = SkyrimRec(0x12DFE);
            Assert.AreEqual(
                "Persistent", rec.GetValue(@"Record Header\Record Flags")
            );
        }

        private static Container NpcAcbs(MainRecord rec) {
            return Child(rec, "Configuration");
        }

        // ACBS Flags bit 7 (PC Level Mult) -> NPCLevelDecider branch.
        [TestCase(0x00000007u, 0)] // Player, flags 0x30
        [TestCase(0x0001325Fu, 1)] // Ahtar, flags 0xB0
        public void NpcLevelBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new NPCLevelDecider().Decide(NpcAcbs(rec))
            );
        }

        [Test]
        public void NpcLevelMissingFlagsReturnsZero() {
            // A GMST record has no direct Flags child.
            var rec = SkyrimRec(0x136A5);
            Assert.AreEqual(0, new NPCLevelDecider().Decide(rec));
        }

        // GMST EDID prefix -> GMSTUnionDecider branch.
        [TestCase(0x000136A5u, 0)] // sBloodSplatterAlpha01OPTFilename
        [TestCase(0x00012F83u, 1)] // iMasserSize
        [TestCase(0x00000DD6u, 2)] // fPickpocketSkillUsesCurve
        [TestCase(0x000B3D8Au, 3)] // bRegenNPCMagickaDuringCast
        public void GmstUnionBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new GMSTUnionDecider().Decide(rec)
            );
        }

        [Test]
        public void GmstUnionNullContainerReturnsOne() {
            Assert.AreEqual(1, new GMSTUnionDecider().Decide(null));
        }

        // The TES4/FO3 legacy decider has no 'b' arm; drive it directly
        // with SSE GMST records (it only reads the EDID sibling).
        [TestCase(0x000136A5u, 0)]
        [TestCase(0x00000DD6u, 2)]
        [TestCase(0x00012F83u, 1)]
        public void Tes4GmstUnionBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new esper.defs.TES4.GMSTUnionDecider().Decide(rec)
            );
        }

        [Test]
        public void Tes4GmstUnionNullContainerReturnsOne() {
            Assert.AreEqual(
                1, new esper.defs.TES4.GMSTUnionDecider().Decide(null));
        }

        private static Container PerkEffect(MainRecord rec, int index) {
            return Child(rec, "Effects").elements[index] as Container;
        }

        // PRKE Type -> PerkDATADecider branch.
        [TestCase(0x0005820Au, 0)] // GoldenTouch, Quest + Stage
        [TestCase(0x0001711Cu, 1)] // DBWellFitted, Ability
        [TestCase(0x000153CDu, 2)] // AlterationDualCasting, Entry Point
        public void PerkDataBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new PerkDATADecider().Decide(PerkEffect(rec, 0))
            );
        }

        [Test]
        public void PerkDataMissingPrkeReturnsZero() {
            var rec = SkyrimRec(0x136A5);
            Assert.AreEqual(0, new PerkDATADecider().Decide(rec));
        }

        private static Container PerkFuncParams(MainRecord rec, int index) {
            return Child(PerkEffect(rec, index), "Function Parameters");
        }

        // EPFT -> EPFDDecider branch; EPFT 2 consults the entry function.
        [TestCase(0x000153CDu, 0, 1)] // DualCasting, Float
        [TestCase(0x000A725Cu, 0, 8)] // AlchemySkillBoosts, AV mult (func 14)
        public void EpfdBranch(uint formId, int effect, int expected) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new EPFDDecider().Decide(PerkFuncParams(rec, effect))
            );
        }

        // Every Skyrim.esm EPFT-2 effect uses function 14, so the other
        // AV-mult arms and the fallback are driven by retargeting a real
        // Function element, then restoring it.
        [TestCase((byte)1, 2)] // Set Value
        [TestCase((byte)5, 8)] // Add Actor Value Mult
        [TestCase((byte)12, 8)] // Set to Actor Value Mult
        [TestCase((byte)13, 8)] // Multiply Actor Value Mult
        public void EpfdRetargetedFunctionBranch(byte funcValue, int expected) {
            var rec = SkyrimRec(0xA725C);
            var effect = PerkEffect(rec, 0);
            var func = effect.GetElement(@"Effect Data\Entry Point\Function")
                as ValueElement;
            var original = func.data;
            try {
                func.data = funcValue;
                Assert.AreEqual(
                    expected, new EPFDDecider().Decide(
                        Child(effect, "Function Parameters"))
                );
            } finally {
                func.data = original;
            }
        }

        [Test]
        public void EpfdNullContainerReturnsZero() {
            Assert.AreEqual(0, new EPFDDecider().Decide(null));
        }

        private static Container PackValue(MainRecord rec, int index) {
            var pd = Child(rec, "Package Data");
            return Child(pd, "Data Input Values").elements[index]
                as Container;
        }

        // ANAM type name -> PubPackCNAMDecider branch.
        [TestCase(0x00000E40u, 0, 0)] // Location
        [TestCase(0x00000E40u, 1, 1)] // Bool
        [TestCase(0x00015EC7u, 1, 2)] // Int
        [TestCase(0x00000F8Du, 2, 3)] // Float
        [TestCase(0x00015F09u, 2, 3)] // ObjectList
        public void PubPackCnamBranch(
            uint formId, int index, int expected
        ) {
            var rec = SkyrimRec(formId);
            Assert.AreEqual(
                expected, new PubPackCNAMDecider().Decide(
                    PackValue(rec, index))
            );
        }

        [Test]
        public void PubPackCnamNullContainerReturnsZero() {
            Assert.AreEqual(0, new PubPackCNAMDecider().Decide(null));
        }

        // PLDT Type -> TypeDecider branch (shared struct decider).
        [TestCase(0x00000E40u, 3)]
        [TestCase(0x00000E41u, 0)]
        public void PackPldtTypeBranch(uint formId, int expected) {
            var rec = SkyrimRec(formId);
            var pldt = Child(PackValue(rec, 0), "Location");
            Assert.AreEqual(expected, new TypeDecider().Decide(pldt));
        }

        [Test]
        public void TypeDeciderNullContainerReturnsZero() {
            Assert.AreEqual(0, new TypeDecider().Decide(null));
        }

        [Test]
        public void FormIdMapRoundTrip() {
            var player = SkyrimRec(0x00000007);
            var ahtar = SkyrimRec(0x0001325F);
            var map = new FormIdMap();
            map.Add(player);
            map.Add(ahtar);
            Assert.AreSame(player, map.Get(player.localFormId));
            Assert.AreSame(ahtar, map.Get(ahtar.localFormId));
            Assert.AreEqual(
                System.Math.Max(player.localFormId, ahtar.localFormId),
                map.highObjectId
            );
        }

        [Test]
        public void EditorIdMapRoundTrip() {
            var player = SkyrimRec(0x00000007);
            var map = new EditorIdMap();
            map.Add(player);
            Assert.AreSame(player, map.Get("Player"));
        }

        [Test]
        public void EditorIdMapSkipsRecordsWithoutEditorId() {
            // Placed references have no EDID.
            var rec = SkyrimRec(0x00DD1);
            Assert.IsNull(rec.GetValue("EDID"));
            var map = new EditorIdMap();
            map.Add(rec);
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => map.Get("Player")
            );
        }

        [Test]
        public void ElementWarningDefaults() {
            var rec = SkyrimRec(0x136A5);
            var edid = rec.GetElement("EDID");
            var warning = new ElementWarning(edid);
            Assert.AreSame(edid, warning.element);
            Assert.AreEqual("", warning.warning);
            var warnings = new ElementWarnings();
            Assert.IsTrue(warnings.Add(edid, "test warning"));
            Assert.AreEqual(1, warnings.Count);
            Assert.AreEqual("test warning", warnings[0].warning);
        }
    }
}
