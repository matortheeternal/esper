using esper;
using esper.defs;
using esper.elements;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests.defs {
    // Direct unit tests for the union deciders ported from xEdit 4.0.4.
    // All of them must be null-safe (missing siblings/headers are common
    // when required elements are auto-created) and the size-based ones
    // must honor the dataSize protocol documented on Decider.Decide.
    public class DeciderBehaviorTests {
        [Test]
        public void NullContainerReturnsDocumentedDefaults() {
            // xEdit initial-Result defaults: most deciders start at 0,
            // PACKPKDT and the form-version deciders start at 1.
            Assert.AreEqual(0, new esper.defs.TES4.EFITOBMEParamDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES4.EFIXParamDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES4.MGEFFAssocItemDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES4.MISCActorValueDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES4.REFRXSEDDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES4.XLOCFillerDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES4.PxDTLocationDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.CreaLevelDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.IMGSSkinDimmerDecider().Decide(null));
            // FO3 MGEFF defaults to branch 1 (xEdit initial Result := 1).
            Assert.AreEqual(1, new esper.defs.FO3.MGEFFAssocItemDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.NOTESNAMDecide().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.NOTETNAMDecide().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.PKDTSpecificFlagsDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.PxDTLocationDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO3.CTDAParam2VATSValueParam().Decide(null));
            Assert.AreEqual(0, new esper.defs.FNV.CTDARunOnDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.AECHDataDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.DoorTriangleDoorTriangleDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.EFSHFormatDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.NOTEDataDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.OMODDataFunctionTypeDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.OMODDataPropertyValue1Decider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.OMODDataPropertyValue2Decider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.SNDRDataDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.SceneActionSoundDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.ScriptFragmentsEmptyScriptDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.TES5.ScriptPropertyDecider().Decide(null));
            Assert.AreEqual(0, new esper.defs.FO4.ScriptPropertyDecider().Decide(null));
            Assert.AreEqual(1, new esper.defs.TES4.PACKPKDTDecider().Decide(null, null));
            Assert.AreEqual(1, new esper.defs.FO4.DeciderFormVersion99().Decide(null));
            Assert.AreEqual(1, new esper.defs.FO4.FormVer78Decider().Decide(null));
        }

        [Test]
        public void SizeBasedDecidersFollowDataSizeProtocol() {
            var pack = new esper.defs.TES4.PACKPKDTDecider();
            Assert.AreEqual(0, pack.Decide(null, 4));
            Assert.AreEqual(1, pack.Decide(null, 8));
            Assert.AreEqual(1, pack.Decide(null, null));
            // Ancestor-size deciders see no enclosing subrecord here.
            Assert.AreEqual(0, new esper.defs.TES4.REFRXSEDDecider().Decide(null, 4));
            Assert.AreEqual(0, new esper.defs.TES4.XLOCFillerDecider().Decide(null, 16));
            Assert.AreEqual(0, new esper.defs.FO3.IMGSSkinDimmerDecider().Decide(null, 140));
            Assert.AreEqual(0, new esper.defs.FO3.PKDTSpecificFlagsDecider().Decide(null, 8));
            Assert.IsNull(DeciderHelpers.AncestorSubrecordSize(null));
        }

        [Test]
        public void TES5AndFO4ScriptPropertyTablesDivergeOnRealData() {
            // FO4 inserted Variable/Struct branches at union indices 6-7,
            // so array types 11-15 shift by 3 instead of TES5's shift by 5.
            // ARecords.esp ACTI[0] carries every property kind (see
            // TestHelpers.ExpectAllProps): index 7 is Object (type 7),
            // index 8 is Array of Object (type 11).
            var session = new Session(Games.SSE, new SessionOptions());
            var plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("ARecords.esp")
            );
            var rec = plugin.GetElement(@"ACTI\[0]");
            Assert.IsNotNull(rec);
            var script = rec.GetElement("VMAD")?.GetElement(@"Scripts\[0]");
            Assert.IsNotNull(script);
            var obj = script.GetElement(@"Properties\[7]") as Container;
            var objArray = script.GetElement(@"Properties\[8]") as Container;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(objArray);
            Assert.AreEqual("PropObject", obj.GetValue("propertyName"));
            Assert.AreEqual("PropObjectArray", objArray.GetValue("propertyName"));

            var strArray = script.GetElement(@"Properties\[10]") as Container;
            Assert.IsNotNull(strArray);
            Assert.AreEqual("PropStringArray", strArray.GetValue("propertyName"));
            Assert.AreEqual((byte?)1, obj.GetData<byte?>("Type"));
            Assert.AreEqual((byte?)11, objArray.GetData<byte?>("Type"));
            Assert.AreEqual((byte?)12, strArray.GetData<byte?>("Type"));

            var tes5 = new esper.defs.TES5.ScriptPropertyDecider();
            var fo4 = new esper.defs.FO4.ScriptPropertyDecider();
            Assert.AreEqual(1, tes5.Decide(obj)); // shared direct range
            Assert.AreEqual(1, fo4.Decide(obj));
            Assert.AreEqual(6, tes5.Decide(objArray)); // 11 - 5, TES5 Struct
            Assert.AreEqual(8, fo4.Decide(objArray)); // 11 - 3, Array of Object
            Assert.AreEqual(7, tes5.Decide(strArray)); // 12 - 5
            Assert.AreEqual(9, fo4.Decide(strArray)); // 12 - 3
        }

        [Test]
        public void GameSessionsResolveGameSpecificDeciders() {
            // FO4 must win over the TES5 fallback with its own table;
            // FNV must inherit FO3 implementations it doesn't override.
            var fo4 = RecordDefCoverageTests.GetSession("FO4");
            Assert.IsInstanceOf<esper.defs.FO4.ScriptPropertyDecider>(
                fo4.definitionManager.GetDecider("ScriptPropertyDecider")
            );
            var tes5 = RecordDefCoverageTests.GetSession("TES5");
            Assert.IsInstanceOf<esper.defs.TES5.ScriptPropertyDecider>(
                tes5.definitionManager.GetDecider("ScriptPropertyDecider")
            );
            var fnv = RecordDefCoverageTests.GetSession("FNV");
            Assert.IsInstanceOf<esper.defs.FO3.CreaLevelDecider>(
                fnv.definitionManager.GetDecider("CreaLevelDecider")
            );
            Assert.IsInstanceOf<esper.defs.FNV.CTDARunOnDecider>(
                fnv.definitionManager.GetDecider("CTDARunOnDecider")
            );
            var tes4 = RecordDefCoverageTests.GetSession("TES4");
            Assert.IsInstanceOf<esper.defs.TES4.MGEFFAssocItemDecider>(
                tes4.definitionManager.GetDecider("MGEFFAssocItemDecider")
            );
        }
    }
}
