using esper;
using esper.data;
using esper.defs;
using esper.defs.TES5;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using esper.warnings;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tests;

namespace Tests.defs {
    // Drives the big TES5 general-purpose formats toward full coverage
    // using real Skyrim.esm records: MoonPhase (CLMT moon phase packing),
    // ActorValue (uint-bits-of-float roundtrip), Alias resolution,
    // QuestStage warnings/display, and REFR navmesh-triangle validation.
    public class FormatDriversTests {
        private class TestStructElement : StructElement {
            private readonly DefinitionManager testManager;
            public TestStructElement(
                StructDef def, DefinitionManager manager
            ) : base(null, def) {
                testManager = manager;
            }
            public override DefinitionManager manager => testManager;
        }

        private static TestStructElement NewStruct(
            string game, string json
        ) {
            var session = RecordDefCoverageTests.GetSession(game);
            var manager = session.definitionManager;
            var def = new StructDef(manager, JObject.Parse(json));
            var element = new TestStructElement(def, manager);
            element.Initialize();
            return element;
        }

        private static IEnumerable<ElementDef> WalkDefs(ElementDef def) {
            yield return def;
            foreach (var child in def.childDefs ?? Enumerable.Empty<ElementDef>())
                foreach (var nested in WalkDefs(child))
                    yield return nested;
        }

        private static IEnumerable<Element> WalkElements(Container container) {
            foreach (var element in container.elements) {
                yield return element;
                if (element is Container child)
                    foreach (var nested in WalkElements(child))
                        yield return nested;
            }
        }

        private static ValueElement FindFormat(MainRecord record, Type format) {
            return WalkElements(record)
                .OfType<ValueElement>()
                .FirstOrDefault(v => v.valueDef?.formatDef?.GetType() == format);
        }

        private static ValueElement Probe(MainRecord record) {
            var queue = new Queue<Element>(record.elements);
            while (queue.Count > 0) {
                var element = queue.Dequeue();
                if (element is ValueElement value) return value;
                if (element is Container container)
                    foreach (var child in container.elements)
                        queue.Enqueue(child);
            }
            throw new Exception(
                "No value element in " + record.signature
            );
        }

        private static PluginFile LoadSkyrim(Session session) {
            var path = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(path))
                Assert.Ignore("Missing local fixture: Skyrim.esm");
            return session.pluginManager.LoadPlugin(path);
        }

        private static void LoadStrings(Session session, PluginFile plugin) {
            var files = new[] {
                "skyrim_english.strings",
                "skyrim_english.dlstrings",
                "skyrim_english.ilstrings",
            }.Select(f => Path.Combine(
                TestHelpers.FixturePath("skyrim_strings"), f
            )).ToList();
            if (files.Any(f => !File.Exists(f)))
                Assert.Ignore("Missing skyrim_strings fixture");
            session.resourceManager.LoadPluginStrings(plugin, files);
        }

        [Test]
        public void MoonPhaseDataToValueCoversAllArms() {
            var manager = RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
            var format = new MoonPhaseFormat(manager, new JObject());
            Assert.AreEqual("No Moon / 5", format.DataToValue(null, 5));
            Assert.AreEqual("Masser / 5", format.DataToValue(null, 69));
            Assert.AreEqual("Secunda / 5", format.DataToValue(null, 133));
            Assert.AreEqual(
                "Masser, Secunda / 3", format.DataToValue(null, 195)
            );
        }

        [Test]
        public void MoonPhaseValueToDataClampsCorrectly() {
            var manager = RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
            var format = new MoonPhaseFormat(manager, new JObject());
            Assert.AreEqual(69, format.ValueToData(null, "Masser / 5"));
            Assert.AreEqual(138, format.ValueToData(null, "Secunda / 10"));
            Assert.AreEqual(
                195, format.ValueToData(null, "Masser, Secunda / 3")
            );
            Assert.AreEqual(7, format.ValueToData(null, "No Moon / 7"));
            Assert.AreEqual(127, format.ValueToData(null, "Masser / 999"));
            Assert.AreEqual(
                "Masser / 5",
                format.DataToValue(null, format.ValueToData(null, "Masser / 5"))
            );
        }

        [Test]
        public void ActorValueRoundtripsThroughEnum() {
            var manager = RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
            var actorValue = WalkDefs(manager.GetRecordDef(
                Signature.FromString("PERK")
            )).OfType<ValueDef>()
                .Select(v => v.formatDef)
                .OfType<ActorValueFormat>()
                .FirstOrDefault();
            Assert.IsNotNull(actorValue, "No ActorValueFormat in PERK def");
            uint bits = 0x3F800000;
            var name = actorValue.DataToValue(null, bits);
            Assert.AreEqual(bits, actorValue.ValueToData(null, name));
            var actorValueEnum = (EnumDef)manager.ResolveDef("ActorValueEnum");
            Assert.AreEqual(
                actorValueEnum.GetSortKey(null, 1L),
                actorValue.GetSortKey(null, bits)
            );
        }

        [Test]
        public void AliasPureArmsWithoutResolution() {
            var manager = RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
            var format = new ConditionAliasFormat(manager, new JObject());
            Assert.AreEqual("None", format.DataToValue(null, -1));
            Assert.AreEqual("4", format.DataToValue(null, 4));
            Assert.AreEqual(123L, format.ValueToData(null, "123"));
            Assert.AreEqual(-1L, format.ValueToData(null, "not a number"));
        }

        [Test]
        public void ConditionAliasResolvesQuestPerRecordType() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var format = new ConditionAliasFormat(
                session.definitionManager, new JObject()
            );
            var qust = plugin.GetRecordByFormId(0x15CC2);
            Assert.AreSame(
                qust,
                format.ResolveQuestRec(Probe(qust))
            );
            var scen = plugin.GetRecordByFormId(0x15F00);
            var quest = format.ResolveQuestRec(Probe(scen));
            Assert.AreEqual("QUST", quest.signature.ToString());
            Assert.AreEqual(0x15CC2, quest.fileFormId);
            Assert.IsNull(format.ResolveQuestRec(
                Probe(plugin.GetRecordByFormId(0xE40))
            ));
            var pack = plugin.GetRecordByFormId(0xEB9);
            var packQuest = (MainRecord)pack.GetElement("@QNAM");
            Assert.IsNotNull(packQuest);
            Assert.AreEqual(0x46EF0, packQuest.fileFormId);
            Assert.AreSame(
                packQuest,
                format.ResolveQuestRec(Probe(pack))
            );
            var info = plugin.GetRecordByFormId(0xDD7);
            var dial = ((GroupRecord)info.container).GetParentRecord();
            var dialQuest = (MainRecord)dial.GetElement("@QNAM");
            Assert.IsNotNull(dialQuest);
            Assert.AreSame(
                dialQuest,
                format.ResolveQuestRec(Probe(info))
            );
            Assert.IsNull(format.ResolveQuestRec(
                Probe(plugin.GetRecordByFormId(0x1B))
            ));
        }

        [Test]
        public void AliasBaseResolverThrows() {
            var manager = RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var format = new AliasFormat(manager, new JObject());
            var header = Probe(plugin.GetRecordByFormId(0x15CC2));
            Assert.Throws<NotImplementedException>(
                () => format.ResolveQuestRec(header)
            );
        }

        [Test]
        public void PackageLocationAliasFollowsRecordQuest() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var format = new PackageLocationAliasFormat(
                session.definitionManager, new JObject()
            );
            Assert.IsNull(format.ResolveQuestRec(null));
            var info = plugin.GetRecordByFormId(0xDD7);
            var stage = FindFormat(info, typeof(CTDAParam2QuestStageFormat));
            Assert.IsNotNull(stage);
            Assert.IsNull(format.ResolveQuestRec(stage));
        }

        [Test]
        public void ExternalAndScriptAliasesResolveUpward() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var manager = session.definitionManager;
            var qust = plugin.GetRecordByFormId(0x15CC2);
            var external = new QuestExternalAliasFormat(manager, new JObject());
            Assert.IsNull(external.ResolveQuestRec(
                Probe(qust)
            ));
            var info = plugin.GetRecordByFormId(0xDD7);
            var stage = FindFormat(info, typeof(CTDAParam2QuestStageFormat));
            Assert.IsNotNull(stage);
            var script = new ScriptObjectAliasFormat(manager, new JObject());
            var aliasEl = (ValueElement)qust.GetElement(
                @"VMAD\Aliases\[0]\Object Union\Object v1\Alias"
            );
            Assert.IsNotNull(aliasEl);
            var formId = script.ResolveQuestRec(aliasEl);
            Assert.AreSame(
                (MainRecord)aliasEl.GetElement(@"..\@FormID"),
                formId
            );
        }

        [Test]
        public void QuestAliasResolvesAgainstAliasList() {
            var skyPath = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(skyPath))
                Assert.Ignore("Missing local fixture: Skyrim.esm");
            var session = new Session(Games.SSE, new SessionOptions {
                resolveAliases = true,
            });
            var plugin = session.pluginManager.LoadPlugin(skyPath);
            var format = new QuestAliasFormat(
                session.definitionManager, new JObject()
            );
            var quest = plugin.GetRecordByFormId(0x1BAEC);
            var header = Probe(quest);
            Assert.AreEqual("99", format.DataToValue(header, 99));
            var aliases = quest.GetElements("Aliases");
            Assert.AreEqual(8, aliases.Count);
            var alias = (Container)aliases[3];
            var index = alias.GetData<uint>("[0]");
            var before = alias.GetValue("Alias Name");
            Assert.AreEqual("banditB", before);
            try {
                Assert.AreEqual(
                    $"{index,3} {before}",
                    format.DataToValue(header, index)
                );
                alias.SetValue("Alias Name", "MyAlias");
                Assert.AreEqual(
                    $"{index,3} MyAlias",
                    format.DataToValue(header, index)
                );
            } finally {
                alias.SetValue("Alias Name", before ?? "");
            }
        }

        [Test]
        public void PerkQuestStageDecodesAndWarns() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            LoadStrings(session, plugin);
            var perk = plugin.GetRecordByFormId(0x5820A);
            var stage = FindFormat(perk, typeof(PerkDATAQuestStageFormat));
            Assert.IsNotNull(stage);
            var format = (PerkDATAQuestStageFormat)stage.valueDef.formatDef;
            var warnings = new ElementWarnings();
            Assert.IsFalse(format.GetWarnings(stage, warnings));
            Assert.IsEmpty(warnings);
            Assert.AreEqual("030", format.DataToValue(stage, 30));
            Assert.AreEqual(30, Convert.ToInt32(format.ValueToData(stage, "030")));
            var quest = WalkElements(perk)
                .OfType<ValueElement>()
                .FirstOrDefault(v => v.data is FormId f &&
                    f.fileFormId == 0x5F596);
            Assert.IsNotNull(quest);
            var questData = quest.data;
            var stageData = stage.data;
            try {
                stage.data = 60000;
                var missing = new ElementWarnings();
                Assert.IsTrue(format.GetWarnings(stage, missing));
                Assert.AreEqual(1, missing.Count);
                Assert.That(missing[0].warning, Does.Contain("not found"));
                Assert.AreEqual("60000", format.DataToValue(stage, 60000));
                quest.data = new FormId(plugin, perk.fileFormId);
                var notQuest = new ElementWarnings();
                Assert.IsTrue(format.GetWarnings(stage, notQuest));
                Assert.That(
                    notQuest[0].warning, Does.Contain("not a Quest record")
                );
            } finally {
                stage.data = stageData;
                quest.data = questData;
            }
        }

        [Test]
        public void QuestStageNullQuestWarns() {
            var manager = RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
            var format = new PerkDATAQuestStageFormat(manager, new JObject());
            var bare = NewStruct("SSE", @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Stage', 'type': 'uint16' }
                ]
            }");
            var stageElement = (ValueElement)bare.GetElement("Stage");
            var warnings = new ElementWarnings();
            Assert.IsTrue(format.GetWarnings(stageElement, warnings));
            Assert.AreEqual(1, warnings.Count);
            Assert.That(
                warnings[0].warning, Does.Contain("Could not resolve")
            );
            var baseless = new QuestStageFormat(manager, new JObject());
            Assert.Throws<NotImplementedException>(
                () => baseless.GetWarnings(stageElement, new ElementWarnings())
            );
        }

        [Test]
        public void InfoQuestStageDecodesAndShowsLogName() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            LoadStrings(session, plugin);
            var info = plugin.GetRecordByFormId(0xDD7);
            var stage = FindFormat(info, typeof(CTDAParam2QuestStageFormat));
            Assert.IsNotNull(stage);
            var format = (CTDAParam2QuestStageFormat)stage.valueDef.formatDef;
            var warnings = new ElementWarnings();
            Assert.IsFalse(format.GetWarnings(stage, warnings));
            Assert.AreEqual("010", format.DataToValue(stage, 10));
            ValueElement param =
                (ValueElement)stage.GetElement(@"..\Parameter #1");
            Assert.IsNotNull(param);
            Assert.AreEqual(
                "QUST", ((ValueElement)param).referencedRecord.signature.ToString()
            );
            var paramData = ((ValueElement)param).data;
            var stageData = stage.data;
            try {
                ((ValueElement)param).data = new FormId(plugin, 0x15D24);
                stage.data = 20;
                var shown = format.DataToValue(stage, 20);
                Assert.That(shown, Does.StartWith("020 "));
                Assert.That(
                    shown, Does.Contain("I've been told to plant the")
                );
            } finally {
                ((ValueElement)param).data = paramData;
                stage.data = stageData;
            }
        }

        [Test]
        public void RefrNavmeshTriangleValidatesIndex() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var plugin = LoadSkyrim(session);
            var refr = plugin.GetRecordByFormId(0x12DFE);
            var triangle = FindFormat(
                refr, typeof(REFRNavmeshTriangleFormat)
            );
            Assert.IsNotNull(triangle);
            var format = (REFRNavmeshTriangleFormat)triangle.valueDef.formatDef;
            Assert.AreEqual("557", format.DataToValue(triangle, 557));
            Assert.AreEqual(557L, format.ValueToData(triangle, "557"));
            var nav = ((Container)triangle.container).elements[0] as ValueElement;
            Assert.IsNotNull(nav);
            Assert.AreEqual(0xD6989, ((FormId)nav.data).fileFormId);
            var clean = new List<ElementWarning>();
            format.GetWarnings(triangle, clean);
            Assert.IsEmpty(clean);
            var navmesh = nav.referencedRecord;
            var triangles = navmesh.GetElement(@"NVNM\Triangles") as Container;
            Assert.IsNotNull(triangles);
            var navData = nav.data;
            var triangleData = triangle.data;
            try {
                triangle.data = triangles.count;
                var missing = new List<ElementWarning>();
                format.GetWarnings(triangle, missing);
                Assert.AreEqual(1, missing.Count);
                Assert.That(missing[0].warning, Does.Contain("triangle"));
                triangle.data = triangles.count - 1;
                var present = new List<ElementWarning>();
                format.GetWarnings(triangle, present);
                Assert.IsEmpty(present);
                nav.data = new FormId(plugin, refr.fileFormId);
                var notNavmesh = new List<ElementWarning>();
                format.GetWarnings(triangle, notNavmesh);
                Assert.AreEqual(1, notNavmesh.Count);
                Assert.That(notNavmesh[0].warning, Does.Contain("Navmesh"));
                nav.data = new FormId(null, 0);
                var orphan = new List<ElementWarning>();
                format.GetWarnings(triangle, orphan);
                Assert.IsEmpty(orphan);
            } finally {
                nav.data = navData;
                triangle.data = triangleData;
            }
        }
    }
}
