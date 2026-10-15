using esper;
using esper.defs;
using esper.defs.FO4;
using esper.plugins;
using esper.resolution;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.defs {
    // Unit tests for the FO4 display formats. Parsing (ValueToData) is
    // pure and null-element-safe; DataToValue falls back to the raw mesh
    // id / decimal text when no containing record is available.
    public class FormatBehaviorTests {
        [Test]
        public void CombinedMeshIDParsesXEditStyleFilenames() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var format = new CombinedMeshIDFormat(
                session.definitionManager, new JObject()
            );
            Assert.AreEqual(
                0x89ABCDEFu,
                ((VariantData.U32)format.ValueToData(null, @"Precombined\00012345_89ABCDEF_OC.nif")).V
            );
            Assert.AreEqual(
                0x89ABCDEFu,
                ((VariantData.U32)format.ValueToData(
                    null, @"Precombined\Fallout4.esm\00012345_89abcdef_OC.nif"
                )).V
            );
            Assert.AreEqual(
                "89ABCDEF", format.DataToValue(null, 0x89ABCDEFu)
            );
        }

        [Test]
        public void CombinedMeshIDRejectsMalformedFilenames() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var format = new CombinedMeshIDFormat(
                session.definitionManager, new JObject()
            );
            Assert.AreEqual(0u, ((VariantData.U32)format.ValueToData(null, null)).V);
            Assert.AreEqual(0u, ((VariantData.U32)format.ValueToData(null, "")).V);
            Assert.AreEqual(0u, ((VariantData.U32)format.ValueToData(null, "foo.nif")).V);
            Assert.AreEqual(
                0u, ((VariantData.U32)format.ValueToData(null, @"Precombined\00012345_OC.nif")).V
            );
            Assert.AreEqual(
                0u, ((VariantData.U32)format.ValueToData(null, @"Precombined\00012345_89ABCDE_OC.nif")).V
            );
            Assert.AreEqual(
                0u, ((VariantData.U32)format.ValueToData(null, @"Precombined\00012345_89ABCDEFQ_OC.nif")).V
            );
            Assert.AreEqual(
                0u, ((VariantData.U32)format.ValueToData(null, @"Precombined\00012345_ZZZZZZZZ_OC.nif")).V
            );
        }

        [Test]
        public void ObjectModPropertyFallsBackToDecimalWithoutRecord() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var format = new ObjectModPropertyFormat(
                session.definitionManager, new JObject()
            );
            Assert.AreEqual(123L, format.ValueToData(null, "123").ToInt64());
            Assert.AreEqual(0L, format.ValueToData(null, "abc").ToInt64());
            Assert.AreEqual("42", format.DataToValue(null, 42));
        }

        [Test]
        public void DivPiFormatConvertsSpecialValues() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var format = new esper.defs.TES5._180DivPiFormat(
                session.definitionManager, new JObject()
            );
            Assert.IsTrue(format.isNumeric);
            Assert.AreEqual("NaN", format.DataToValue(null, float.NaN));
            Assert.AreEqual(
                "Inf", format.DataToValue(null, float.PositiveInfinity)
            );
            Assert.AreEqual(
                "Inf", format.DataToValue(null, float.NegativeInfinity)
            );
            Assert.AreEqual(
                "Default", format.DataToValue(null, float.MaxValue)
            );
            Assert.AreEqual("Min", format.DataToValue(null, -float.MaxValue));
            Assert.AreEqual("57.295780", format.DataToValue(null, 1.0f));
            Assert.Throws<System.NotImplementedException>(
                () => format.ValueToData(null, "57.29578")
            );
        }

        [Test]
        public void MovtAnimThresholdsDecodeThroughProductionPath() {
            var skyrim = Fixture.Master(Games.SSE, "Skyrim.esm");
            // NPC_Default_MT stores MaxValue floats, which the format
            // renders as Default.
            var rec = skyrim.GetRecordByFormId(0x3580D);
            Assert.IsNotNull(rec);
            Assert.AreEqual("Default", rec.GetValue(@"INAM\Directional"));
            Assert.AreEqual(
                "Default", rec.GetValue(@"INAM\Rotation Speed")
            );
        }
    }
}
