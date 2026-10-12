using esper.defs.FO4;
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
                (object)format.ValueToData(null, @"Precombined\00012345_89ABCDEF_OC.nif")
            );
            Assert.AreEqual(
                0x89ABCDEFu,
                (object)format.ValueToData(
                    null, @"Precombined\Fallout4.esm\00012345_89abcdef_OC.nif"
                )
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
            Assert.AreEqual(0u, (object)format.ValueToData(null, null));
            Assert.AreEqual(0u, (object)format.ValueToData(null, ""));
            Assert.AreEqual(0u, (object)format.ValueToData(null, "foo.nif"));
            Assert.AreEqual(
                0u, (object)format.ValueToData(null, @"Precombined\00012345_OC.nif")
            );
            Assert.AreEqual(
                0u, (object)format.ValueToData(null, @"Precombined\00012345_89ABCDE_OC.nif")
            );
            Assert.AreEqual(
                0u, (object)format.ValueToData(null, @"Precombined\00012345_89ABCDEFQ_OC.nif")
            );
            Assert.AreEqual(
                0u, (object)format.ValueToData(null, @"Precombined\00012345_ZZZZZZZZ_OC.nif")
            );
        }

        [Test]
        public void ObjectModPropertyFallsBackToDecimalWithoutRecord() {
            var session = RecordDefCoverageTests.GetSession("FO4");
            var format = new ObjectModPropertyFormat(
                session.definitionManager, new JObject()
            );
            Assert.AreEqual(123L, (object)format.ValueToData(null, "123"));
            Assert.AreEqual(0L, (object)format.ValueToData(null, "abc"));
            Assert.AreEqual("42", format.DataToValue(null, 42));
        }
    }
}
