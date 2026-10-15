using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.io;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.defs {
    [TestFixture]
    public class ValueDefCoverageTests {
        private static DefinitionManager SseManager() {
            return RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
        }

        private static JObject Named(string name) {
            return JObject.Parse(
                "{'name':'" + name + "'}"
            );
        }

        private static PluginFile LoadSkyrim(Session session) {
            return Fixture.LoadFreshInto(session, Games.SSE, "Skyrim.esm");
        }

        private static PluginFile LoadSharedSkyrim() {
            return Fixture.Master(Games.SSE, "Skyrim.esm");
        }

        private static void LoadStrings(Session session, PluginFile plugin) {
            Fixture.LoadStrings(Games.SSE, plugin, "Skyrim.esm");
        }

        private static string WriteBytes(
            PluginFile plugin, Action<PluginFileOutput> write
        ) {
            var path = Path.GetTempFileName();
            try {
                var output = new PluginFileOutput(path, plugin);
                write(output);
                output.writer.Flush();
                output.writer.Close();
                return Convert.ToHexString(File.ReadAllBytes(path));
            } finally {
                File.Delete(path);
            }
        }

        [Test]
        public void Int8PropsReadDefaultAndSortKey() {
            var def = new Int8Def(SseManager(), Named("I8"));
            Assert.AreEqual("int8", Int8Def.defId);
            Assert.AreEqual(XEDefType.dtInteger, def.valueDefType);
            Assert.AreEqual(SmashType.stInteger, def.smashType);
            Assert.IsTrue(def.isNumeric);
            Assert.AreEqual(1, def.size);
            var source = new RecordSource(null, new byte[] { 0xFF });
            Assert.AreEqual((sbyte)-1, def.ReadData(source, null));
            Assert.AreEqual((sbyte)0, def.DefaultData());
            Assert.AreEqual("7F", def.DataToSortKey((sbyte)-1));
            Assert.AreEqual("00", def.DataToSortKey((sbyte)-128));
            Assert.AreEqual("FF", def.DataToSortKey((sbyte)127));
        }

        [Test]
        public void Int8SetDataClampsWhenEnabled() {
            var def = new Int8Def(SseManager(), Named("I8"));
            var element = new ValueElement(null, def);
            def.SetData(element, 999L);
            Assert.AreEqual((sbyte)127, element._data);
            def.SetData(element, -999L);
            Assert.AreEqual((sbyte)-128, element._data);
            def.SetData(element, 5L);
            Assert.AreEqual((sbyte)5, element._data);
        }

        [Test]
        public void Int8SetDataPassesThroughWhenClampDisabled() {
            var session = new Session(Games.SSE, new SessionOptions {
                clampIntegerValues = false
            });
            var def = new Int8Def(
                session.definitionManager, Named("I8")
            );
            var element = new ValueElement(null, def);
            def.SetData(element, 999L);
            Assert.AreEqual((sbyte)-25, element._data);
        }

        [Test]
        public void Int8WriteDataHandlesSbyteAndFallback() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSharedSkyrim();
            var def = new Int8Def(SseManager(), Named("I8"));
            var element = new ValueElement(null, def);
            element._data = (sbyte)5;
            Assert.AreEqual(
                "05", WriteBytes(plugin, o => def.WriteData(element, o))
            );
            element._data = 5;
            Assert.AreEqual(
                "00", WriteBytes(plugin, o => def.WriteData(element, o))
            );
        }

        [Test]
        public void Int8GetValueNullAndSetValue() {
            var def = new Int8Def(SseManager(), Named("I8"));
            var element = new ValueElement(null, def);
            Assert.AreEqual("", def.GetValue(element));
            element._data = (sbyte)5;
            Assert.AreEqual("5", def.GetValue(element));
            def.SetValue(element, "42");
            Assert.AreEqual((sbyte)42, element._data);
        }

        [Test]
        public void Int8FormatArmsUseFormatDef() {
            var def = new Int8Def(SseManager(), JObject.Parse(
                "{'name':'I8F','format':{'type':'enum','options':" +
                "{'0':'A','1':'B'}}}"
            ));
            Assert.IsNotNull(def.formatDef);
            var element = new ValueElement(null, def);
            element._data = 1L;
            Assert.AreEqual("B", def.GetValue(element));
            def.SetValue(element, "A");
            Assert.AreEqual("A", def.GetValue(element));
        }

        [Test]
        public void Int8ToJObjectBaseArms() {
            var def = new Int8Def(SseManager(), Named("I8"));
            var full = def.ToJObject();
            var slim = def.ToJObject(false);
            Assert.IsNotNull(full);
            Assert.IsNotNull(slim);
            Assert.Greater(full.Count, slim.Count);
        }

        [Test]
        public void Int8SizeSortKeyAndZeroMask() {
            var def = new Int8Def(SseManager(), Named("I8"));
            var formDef = new FormIdDef(SseManager(), Named("F"));
            var element = new ValueElement(null, def);
            var formElement = new ValueElement(null, formDef);
            Assert.AreEqual(3, formDef.GetSize(formElement) - def.GetSize(element));
            element._data = (sbyte)-1;
            Assert.AreEqual("7F", def.GetSortKey(element));
            var masked = new Int8Def(SseManager(), JObject.Parse(
                "{'name':'I8Z','zeroSortKey':true}"
            ));
            var maskedKey = masked.GetSortKey(element);
            Assert.AreEqual(2, maskedKey.Length);
            Assert.IsTrue(maskedKey.All(c => c == '0'));
        }

        [Test]
        public void UInt8PropsReadDefaultSortKeyAndClamp() {
            var def = new UInt8Def(SseManager(), Named("U8"));
            Assert.AreEqual(XEDefType.dtInteger, def.valueDefType);
            Assert.AreEqual(1, def.size);
            var source = new RecordSource(null, new byte[] { 0xFF });
            Assert.AreEqual((byte)255, def.ReadData(source, null));
            Assert.AreEqual((byte)0, def.DefaultData());
            Assert.AreEqual("FF", def.DataToSortKey((byte)255));
            Assert.AreEqual("00", def.DataToSortKey((byte)0));
            var element = new ValueElement(null, def);
            def.SetData(element, 999L);
            Assert.AreEqual((byte)255, element._data);
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSharedSkyrim();
            element._data = (byte)7;
            Assert.AreEqual(
                "07",
                WriteBytes(plugin, o => def.WriteData(element, o))
            );
            Assert.IsNotNull(def.ToJObject());
        }

        [Test]
        public void BareValueDefBaseThrowsAndDefaults() {
            var def = new ValueDef(SseManager(), Named("Base"));
            Assert.Throws<NotImplementedException>(
                () => { var _ = def.valueDefType; }
            );
            Assert.IsFalse(def.isNumeric);
            var element = new ValueElement(null, def);
            var source = new RecordSource(null, new byte[] { 0x01 });
            Assert.Throws<NotImplementedException>(
                () => def.ReadData(source, null)
            );
            Assert.Throws<NotImplementedException>(
                () => def.DefaultData()
            );
            Assert.Throws<NotImplementedException>(
                () => def.WriteData(element, null)
            );
            var int8 = new Int8Def(SseManager(), Named("I8"));
            var int8Element = new ValueElement(null, int8);
            Assert.AreEqual(
                int8.GetSize(int8Element) - 1, def.GetSize(element)
            );
        }

        [Test]
        public void BareValueDefToJObjectSizeArms() {
            var sized = new ValueDef(SseManager(), JObject.Parse(
                "{'name':'S','size':4}"
            ));
            var src = sized.ToJObject();
            Assert.AreEqual(4, (int)src["size"]);
            var slim = sized.ToJObject(false);
            Assert.AreEqual(1, slim.Count);
            Assert.IsNull(slim["size"]);
            Assert.IsNull(slim["name"]);
            Assert.IsNotNull(slim["id"]);
            var bare = new ValueDef(SseManager(), Named("B"));
            var bareSrc = bare.ToJObject();
            Assert.AreEqual(1, bareSrc.Count);
            Assert.AreEqual("B", (string)bareSrc["name"]);
        }

        [Test]
        public void BareValueDefSetValueAndGetValueArms() {
            var def = new ValueDef(SseManager(), Named("Base"));
            var element = new ValueElement(null, def);
            Assert.AreEqual("", def.GetValue(element));
            def.SetValue(element, "42");
            Assert.AreEqual(42L, element._data);
            Assert.AreEqual("42", def.GetValue(element));
            var formatted = new ValueDef(SseManager(), JObject.Parse(
                "{'name':'F','format':{'type':'enum','options':" +
                "{'0':'A','1':'B'}}}"
            ));
            var formattedElement = new ValueElement(null, formatted);
            formattedElement._data = 1L;
            Assert.AreEqual("B", formatted.GetValue(formattedElement));
            formatted.SetValue(formattedElement, "A");
            Assert.AreEqual("A", formatted.GetValue(formattedElement));
        }

        [Test]
        public void CustomSortKeyDelegatesToFormat() {
            var manager = SseManager();
            var owner = WalkDefs(manager.GetRecordDef(
                Signature.FromString("PERK")
            )).OfType<ValueDef>().FirstOrDefault(d =>
                d.formatDef is esper.defs.TES5.ActorValueFormat
            );
            Assert.IsNotNull(owner, "No ActorValueFormat value def found");
            var element = new ValueElement(null, owner);
            element._data = 0x3F800000u;
            var key = owner.GetSortKey(element);
            Assert.IsNotNull(key);
            var again = new ValueElement(null, owner);
            again._data = 0x3F800000u;
            Assert.AreEqual(key, owner.GetSortKey(again));
        }

        private static IEnumerable<ElementDef> WalkDefs(ElementDef root) {
            yield return root;
            foreach (var child in root.childDefs ?? Enumerable.Empty<ElementDef>())
                foreach (var nested in WalkDefs(child))
                    yield return nested;
        }

        [Test]
        public void FormIdPropsReadDefaultAndValue() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSharedSkyrim();
            var def = new FormIdDef(SseManager(), Named("F"));
            Assert.AreEqual(XEDefType.dtIntegerFormater, def.valueDefType);
            Assert.AreEqual(4, def.size);
            Assert.IsTrue(def.canContainFormIds);
            var refr = plugin.GetRecordByFormId(0x12DFE);
            var source = new RecordSource(
                refr, new byte[] { 0xEA, 0x06, 0x00, 0x00 }
            );
            var fid = def.ReadData(source, null);
            Assert.AreEqual(0x6EAu, ((FormId)fid).fileFormId);
            var empty = def.DefaultData();
            Assert.AreEqual(0u, ((FormId)empty).fileFormId);
            var nav = (ValueElement)refr.GetElement(
                @"XNDP\Navigation Mesh"
            );
            Assert.AreEqual("{Skyrim.esm:0D6989}", def.GetValue(nav));
            Assert.AreEqual(
                "000D6989", def.DataToSortKey(nav.data)
            );
            Assert.AreEqual("00000000", def.DataToSortKey(null));
        }

        [Test]
        public void FormIdSetValueSkipsValidationByDefault() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSharedSkyrim();
            var def = new FormIdDef(SseManager(), Named("F"));
            var element = new ValueElement(
                plugin.GetRecordByFormId(0x12DFE), def
            );
            def.SetValue(element, "{Skyrim.esm:000EA6}");
            Assert.AreEqual(0xEA6u, ((FormId)element._data).fileFormId);
        }

        [Test]
        public void FormIdWriteDataFormIdAndFallback() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSharedSkyrim();
            var refr = plugin.GetRecordByFormId(0x12DFE);
            var navDef = (FormIdDef)((ValueElement)refr.GetElement(
                @"XNDP\Navigation Mesh"
            )).valueDef;
            var nav = (ValueElement)refr.GetElement(@"XNDP\Navigation Mesh");
            var expected = BitConverter.ToString(
                BitConverter.GetBytes(((FormId)nav.data).fileFormId)
            ).Replace("-", "");
            Assert.AreEqual(
                expected,
                WriteBytes(plugin, o => navDef.WriteData(nav, o))
            );
            var fresh = new ValueElement(null, navDef);
            fresh._data = (UInt32)0x1234;
            Assert.AreEqual(
                "00000000",
                WriteBytes(plugin, o => navDef.WriteData(fresh, o))
            );
        }

        [Test]
        public void FormIdToJObjectKeyArms() {
            var full = new FormIdDef(SseManager(), JObject.Parse(
                "{'name':'F','validateFlstRefs':true,'persistent':true}"
            ));
            var src = full.ToJObject();
            Assert.AreEqual(true, (bool)src["validateFlstRefs"]);
            Assert.AreEqual(true, (bool)src["persistent"]);
            Assert.AreEqual("formId", (string)src["type"]);
            var bare = new FormIdDef(SseManager(), Named("F"));
            var bareSrc = bare.ToJObject();
            Assert.IsNull(bareSrc["signatures"]);
            Assert.IsNull(bareSrc["validateFlstRefs"]);
            Assert.IsNull(bareSrc["persistent"]);
            var masked = new FormIdDef(SseManager(), JObject.Parse(
                "{'name':'Z','zeroSortKey':true}"
            ));
            var maskedObj = masked.ToJObject();
            Assert.AreEqual(3, maskedObj.Count);
            Assert.AreEqual("Z", (string)maskedObj["name"]);
            Assert.IsTrue((bool)maskedObj["zeroSortKey"]);
            Assert.AreEqual("formId", (string)maskedObj["type"]);
            var sized = new FormIdDef(SseManager(), JObject.Parse(
                "{'name':'Z','size':4}"
            ));
            var sizedObj = sized.ToJObject();
            Assert.AreEqual(3, sizedObj.Count);
            Assert.AreEqual(4, (int)sizedObj["size"]);
            Assert.AreEqual("formId", (string)sizedObj["type"]);
        }

        [Test]
        public void FormIdValidateRefMatrix() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSharedSkyrim();
            var navDef = new FormIdDef(SseManager(), JObject.Parse(
                "{'name':'Nav','signatures':['NAVM']}"
            ));
            var navm = plugin.records.First(
                r => r.signature.ToString() == "NAVM"
            );
            var perk = plugin.GetRecordByFormId(0x5820A);
            var element = new ValueElement(perk, navDef);
            Assert.Throws<Exception>(
                () => navDef.ValidateRef(element, perk)
            );
            navDef.ValidateRef(element, navm);
            var persistent = new FormIdDef(SseManager(), JObject.Parse(
                "{'name':'P','signatures':['ACTI'],'persistent':true}"
            ));
            var acti = plugin.GetRecordByFormId(0x1B);
            Assert.Throws<Exception>(
                () => persistent.ValidateRef(element, acti)
            );
            var persistentRef = plugin.records.FirstOrDefault(
                r => r.signature.ToString() == "REFR" &&
                    r.GetRecordFlag("Persistent")
            );
            var persistentPass = new FormIdDef(SseManager(), JObject.Parse(
                "{'name':'PP','signatures':['REFR'],'persistent':true}"
            ));
            if (persistentRef != null)
                persistentPass.ValidateRef(
                    new ValueElement(persistentRef, persistentPass),
                    persistentRef
                );
        }

        [Test]
        public void FormIdEnforceSessionValidatesOnSetValue() {
            var session = new Session(Games.SSE, new SessionOptions {
                enforceExpectedReferences = true
            });
            var plugin = LoadSkyrim(session);
            var rec = plugin.GetRecordByFormId(0x5820A);
            var def = new FormIdDef(
                session.definitionManager, JObject.Parse(
                    "{'name':'Nav','signatures':['NAVM']}"
                )
            );
            var element = new ValueElement(rec, def);
            var navm = plugin.records.First(
                r => r.signature.ToString() == "NAVM"
            );
            var target = "{Skyrim.esm:" +
                navm.fileFormId.ToString("X6") + "}";
            def.SetValue(element, target);
            Assert.AreEqual(navm.fileFormId, ((FormId)element._data).fileFormId);
            Assert.Throws<Exception>(
                () => def.SetValue(element, "{Skyrim.esm:00001B}")
            );
        }

        [Test]
        public void FormIdValidateFormListRefsMatrix() {
            var plugin = LoadSharedSkyrim();
            MainRecord list = null;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "FLST") continue;
                var found = rec.GetElements("FormIDs");
                if (found == null || found.Count == 0) continue;
                var clean = true;
                foreach (var item in found) {
                    var fid = ((ValueElement)item)._data as FormId;
                    if (fid == null) continue;
                    var target = plugin.GetRecordByFormId(fid.fileFormId);
                    if (target == null) continue;
                    if (target.signature.ToString() != "FLST") {
                        clean = false;
                        break;
                    }
                }
                if (!clean) continue;
                list = rec;
                break;
            }
            Assert.IsNotNull(list, "No all-FLST FormIDs list found");
            var def = new FormIdDef(
                Synthetic.GetSession(Games.SSE).definitionManager, JObject.Parse(
                    "{'name':'L','signatures':['FLST']," +
                    "'validateFlstRefs':true}"
                )
            );
            var element = new ValueElement(list, def);
            var acti = plugin.GetRecordByFormId(0x1B);
            def.ValidateFormListRefs(element, acti);
            var entry = list.GetElements("FormIDs")[0];
            var before = ((ValueElement)entry)._data;
            try {
                ((ValueElement)entry)._data = null;
                def.ValidateFormListRefs(element, list);
                ((ValueElement)entry)._data = new FormId(plugin, 0x5820A);
                Assert.Throws<Exception>(
                    () => def.ValidateFormListRefs(element, list)
                );
            } finally {
                ((ValueElement)entry)._data = before;
            }
        }

        private static MainRecord CleanFlstList(PluginFile plugin) {
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "FLST") continue;
                var found = rec.GetElements("FormIDs");
                if (found == null || found.Count == 0) continue;
                var clean = true;
                foreach (var item in found) {
                    var fid = ((ValueElement)item)._data as FormId;
                    if (fid == null) continue;
                    var target = plugin.GetRecordByFormId(fid.fileFormId);
                    if (target != null &&
                        target.signature.ToString() != "FLST") {
                        clean = false;
                        break;
                    }
                }
                if (clean) return rec;
            }
            return null;
        }

        [Test]
        public void FormIdValidateRefDelegatesToFormListRefs() {
            var plugin = LoadSharedSkyrim();
            var list = CleanFlstList(plugin);
            Assert.IsNotNull(list, "No clean FLST list found");
            var def = new FormIdDef(
                Synthetic.GetSession(Games.SSE).definitionManager, JObject.Parse(
                    "{'name':'L','signatures':['FLST']," +
                    "'validateFlstRefs':true}"
                )
            );
            var element = new ValueElement(list, def);
            def.ValidateRef(element, list);
        }

        [Test]
        public void StringValueDefTypeArms() {
            var plain = new StringDef(SseManager(), Named("S"));
            var prefixed = new StringDef(SseManager(), JObject.Parse(
                "{'name':'S','prefix':1}"
            ));
            var localized = new StringDef(SseManager(), JObject.Parse(
                "{'name':'S','localized':true}"
            ));
            Assert.AreEqual(XEDefType.dtString, plain.valueDefType);
            Assert.AreEqual(XEDefType.dtLenString, prefixed.valueDefType);
            Assert.AreEqual(XEDefType.dtLString, localized.valueDefType);
            Assert.AreEqual(SmashType.stString, plain.smashType);
            Assert.AreEqual(string.Empty, plain.DefaultData());
        }

        [Test]
        public void StringSetDataThrowsOnNullAndFixedMismatch() {
            var def = new StringDef(SseManager(), Named("S"));
            var element = new ValueElement(null, def);
            Assert.Throws<Exception>(() => def.SetData(element, null));
            var fixed4 = new StringDef(SseManager(), JObject.Parse(
                "{'name':'S','size':4}"
            ));
            var fixedElement = new ValueElement(null, fixed4);
            Assert.Throws<Exception>(
                () => fixed4.SetData(fixedElement, "ABC")
            );
            fixed4.SetData(fixedElement, "ABCD");
            Assert.AreEqual("ABCD", fixedElement._data);
        }

        [Test]
        public void StringGetValueNullPlainAndLocalized() {
            var plugin = LoadSharedSkyrim();
            LoadStrings(Synthetic.GetSession(Games.SSE), plugin);
            var def = new StringDef(SseManager(), Named("S"));
            var element = new ValueElement(null, def);
            Assert.IsNull(def.GetValue(element));
            element._data = "Hi";
            Assert.AreEqual("Hi", def.GetValue(element));
            MainRecord named = null;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "NPC_") continue;
                var full = rec.GetElement("FULL");
                if (full is ValueElement v && v._data is LocalizedString) {
                    named = rec;
                    break;
                }
            }
            Assert.IsNotNull(named, "No localized FULL found");
            var fullElement = (ValueElement)named.GetElement("FULL");
            var text = ((StringDef)fullElement.valueDef).GetValue(fullElement);
            Assert.IsFalse(string.IsNullOrEmpty(text));
        }

        [Test]
        public void StringGetSizeLocalizedStringArm() {
            var plugin = LoadSharedSkyrim();
            var int8 = new Int8Def(
                Synthetic.GetSession(Games.SSE).definitionManager, Named("I8")
            );
            var int8Element = new ValueElement(null, int8);
            var def = new StringDef(
                Synthetic.GetSession(Games.SSE).definitionManager, Named("S")
            );
            var element = new ValueElement(null, def);
            element._data = new LocalizedString(plugin, 1);
            Assert.AreEqual(
                3, def.GetSize(element) - int8.GetSize(int8Element)
            );
        }

        [Test]
        public void StringWriteDataPlainPrefixAndLocalized() {
            var plugin = LoadSharedSkyrim();
            var def = new StringDef(
                Synthetic.GetSession(Games.SSE).definitionManager, Named("S")
            );
            var element = new ValueElement(null, def);
            element._data = "AB";
            Assert.AreEqual(
                "414200",
                WriteBytes(plugin, o => def.WriteData(element, o))
            );
            var prefixed = new StringDef(
                Synthetic.GetSession(Games.SSE).definitionManager,
                JObject.Parse("{'name':'S','prefix':1}")
            );
            var prefixedElement = new ValueElement(null, prefixed);
            prefixedElement._data = "AB";
            Assert.AreEqual(
                "024142",
                WriteBytes(plugin, o => prefixed.WriteData(prefixedElement, o))
            );
            var lsElement = new ValueElement(null, def);
            lsElement._data = new LocalizedString(plugin, 0x1234);
            Assert.AreEqual(
                "34120000",
                WriteBytes(plugin, o => def.WriteData(lsElement, o))
            );
        }

        [Test]
        public void StringToJObjectFlagArms() {
            var plain = new StringDef(SseManager(), Named("S"));
            Assert.IsNull(plain.ToJObject()["localized"]);
            var localized = new StringDef(SseManager(), JObject.Parse(
                "{'name':'S','localized':true}"
            ));
            Assert.AreEqual(true, (bool)localized.ToJObject()["localized"]);
            var prefixed = new StringDef(SseManager(), JObject.Parse(
                "{'name':'S','prefix':2,'padding':1,'keepCase':true}"
            ));
            var src = prefixed.ToJObject();
            Assert.AreEqual(2, (int)src["prefix"]);
            Assert.AreEqual(1, (int)src["padding"]);
            Assert.AreEqual(true, (bool)src["keepCase"]);
        }
    }
}
