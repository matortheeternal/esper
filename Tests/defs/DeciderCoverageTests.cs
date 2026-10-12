using esper;
using esper.defs.TES5;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var path = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(path)) Assert.Ignore("Skyrim.esm is not present.");
            skyrim = session.pluginManager.LoadPlugin(path);
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
    }
}
