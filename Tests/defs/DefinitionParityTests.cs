using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.defs {
    public class DefinitionParityTests {
        private class TestStructElement : StructElement {
            private readonly DefinitionManager definitionManager;

            public override DefinitionManager manager => definitionManager;

            public TestStructElement(
                StructDef def, DefinitionManager definitionManager
            ) : base(null, def) {
                this.definitionManager = definitionManager;
            }
        }

        private static readonly (CTDAFunctionParamType, int)[] tes4Params = {
            (CTDAFunctionParamType.ptNone, 1),
            (CTDAFunctionParamType.ptInteger, 2),
            (CTDAFunctionParamType.ptVariableName, 3),
            (CTDAFunctionParamType.ptSex, 4),
            (CTDAFunctionParamType.ptActorValue, 5),
            (CTDAFunctionParamType.ptCrimeType, 6),
            (CTDAFunctionParamType.ptAxis, 7),
            (CTDAFunctionParamType.ptFormType, 8),
            (CTDAFunctionParamType.ptQuestStage, 9),
            (CTDAFunctionParamType.ptObjectReference, 10),
            (CTDAFunctionParamType.ptInventoryObject, 11),
            (CTDAFunctionParamType.ptActor, 12),
            (CTDAFunctionParamType.ptQuest, 13),
            (CTDAFunctionParamType.ptFaction, 14),
            (CTDAFunctionParamType.ptCell, 15),
            (CTDAFunctionParamType.ptClass, 16),
            (CTDAFunctionParamType.ptRace, 17),
            (CTDAFunctionParamType.ptActorBase, 18),
            (CTDAFunctionParamType.ptGlobal, 19),
            (CTDAFunctionParamType.ptWeather, 20),
            (CTDAFunctionParamType.ptPackage, 21),
            (CTDAFunctionParamType.ptOwner, 22),
            (CTDAFunctionParamType.ptFurniture, 24),
            (CTDAFunctionParamType.ptMagicItem, 25),
            (CTDAFunctionParamType.ptMagicEffect, 26),
            (CTDAFunctionParamType.ptWorldspace, 27),
            (CTDAFunctionParamType.ptReferencableObject, 28)
        };

        private static readonly (CTDAFunctionParamType, int)[] fo3Params = {
            (CTDAFunctionParamType.ptNone, 1),
            (CTDAFunctionParamType.ptInteger, 2),
            (CTDAFunctionParamType.ptVariableName, 3),
            (CTDAFunctionParamType.ptSex, 4),
            (CTDAFunctionParamType.ptActorValue, 5),
            (CTDAFunctionParamType.ptCrimeType, 6),
            (CTDAFunctionParamType.ptAxis, 7),
            (CTDAFunctionParamType.ptQuestStage, 8),
            (CTDAFunctionParamType.ptMiscStat, 9),
            (CTDAFunctionParamType.ptAlignment, 10),
            (CTDAFunctionParamType.ptEquipType, 11),
            (CTDAFunctionParamType.ptFormType, 12),
            (CTDAFunctionParamType.ptCriticalStage, 13),
            (CTDAFunctionParamType.ptObjectReference, 14),
            (CTDAFunctionParamType.ptInventoryObject, 15),
            (CTDAFunctionParamType.ptActor, 16),
            (CTDAFunctionParamType.ptVoiceType, 17),
            (CTDAFunctionParamType.ptIdleForm, 18),
            (CTDAFunctionParamType.ptFormList, 19),
            (CTDAFunctionParamType.ptQuest, 20),
            (CTDAFunctionParamType.ptFaction, 21),
            (CTDAFunctionParamType.ptCell, 24),
            (CTDAFunctionParamType.ptClass, 25),
            (CTDAFunctionParamType.ptRace, 26),
            (CTDAFunctionParamType.ptActorBase, 27),
            (CTDAFunctionParamType.ptGlobal, 28),
            (CTDAFunctionParamType.ptWeather, 29),
            (CTDAFunctionParamType.ptPackage, 30),
            (CTDAFunctionParamType.ptEncounterZone, 31),
            (CTDAFunctionParamType.ptPerk, 32),
            (CTDAFunctionParamType.ptOwner, 33),
            (CTDAFunctionParamType.ptFurniture, 34),
            (CTDAFunctionParamType.ptMagicItem, 35),
            (CTDAFunctionParamType.ptMagicEffect, 36),
            (CTDAFunctionParamType.ptWorldspace, 37),
            (CTDAFunctionParamType.ptVATSValueFunction, 38),
            (CTDAFunctionParamType.ptVATSValueParam, 39),
            (CTDAFunctionParamType.ptReferencableObject, 44),
            (CTDAFunctionParamType.ptRegion, 47)
        };

        private static readonly (CTDAFunctionParamType, int)[] fo4Params = {
            (CTDAFunctionParamType.ptNone, 1),
            (CTDAFunctionParamType.ptInteger, 3),
            (CTDAFunctionParamType.ptFloat, 4),
            (CTDAFunctionParamType.ptActor, 5),
            (CTDAFunctionParamType.ptActorBase, 6),
            (CTDAFunctionParamType.ptActorValue, 7),
            (CTDAFunctionParamType.ptAdvanceAction, 8),
            (CTDAFunctionParamType.ptAlias, 9),
            (CTDAFunctionParamType.ptAlignment, 10),
            (CTDAFunctionParamType.ptAssociationType, 11),
            (CTDAFunctionParamType.ptAxis, 12),
            (CTDAFunctionParamType.ptCastingSource, 13),
            (CTDAFunctionParamType.ptCell, 14),
            (CTDAFunctionParamType.ptClass, 15),
            (CTDAFunctionParamType.ptCrimeType, 16),
            (CTDAFunctionParamType.ptCriticalStage, 17),
            (CTDAFunctionParamType.ptEncounterZone, 18),
            (CTDAFunctionParamType.ptEquipType, 19),
            (CTDAFunctionParamType.ptEvent, 20),
            (CTDAFunctionParamType.ptEventData, 21),
            (CTDAFunctionParamType.ptFaction, 22),
            (CTDAFunctionParamType.ptFormList, 23),
            (CTDAFunctionParamType.ptFormType, 24),
            (CTDAFunctionParamType.ptFurniture, 25),
            (CTDAFunctionParamType.ptFurnitureAnim, 26),
            (CTDAFunctionParamType.ptFurnitureEntry, 27),
            (CTDAFunctionParamType.ptGlobal, 28),
            (CTDAFunctionParamType.ptIdleForm, 29),
            (CTDAFunctionParamType.ptInventoryObject, 30),
            (CTDAFunctionParamType.ptKeyword, 31),
            (CTDAFunctionParamType.ptLocation, 32),
            (CTDAFunctionParamType.ptMagicEffect, 33),
            (CTDAFunctionParamType.ptMagicItem, 34),
            (CTDAFunctionParamType.ptMiscStat, 35),
            (CTDAFunctionParamType.ptObjectReference, 36),
            (CTDAFunctionParamType.ptOwner, 37),
            (CTDAFunctionParamType.ptPackage, 38),
            (CTDAFunctionParamType.ptPackdata, 39),
            (CTDAFunctionParamType.ptPerk, 40),
            (CTDAFunctionParamType.ptQuest, 41),
            (CTDAFunctionParamType.ptQuestStage, 42),
            (CTDAFunctionParamType.ptRace, 43),
            (CTDAFunctionParamType.ptReferencableObject, 44),
            (CTDAFunctionParamType.ptRefType, 45),
            (CTDAFunctionParamType.ptRegion, 46),
            (CTDAFunctionParamType.ptScene, 47),
            (CTDAFunctionParamType.ptSex, 48),
            (CTDAFunctionParamType.ptShout, 49),
            (CTDAFunctionParamType.ptVariableName, 50),
            (CTDAFunctionParamType.ptVATSValueFunction, 51),
            (CTDAFunctionParamType.ptVATSValueParam, 52),
            (CTDAFunctionParamType.ptVoiceType, 53),
            (CTDAFunctionParamType.ptWardState, 54),
            (CTDAFunctionParamType.ptWeather, 55),
            (CTDAFunctionParamType.ptWorldspace, 56)
        };

        private static StructElement NewStruct(Game game, string json) {
            var manager = Synthetic.GetSession(game).definitionManager;
            var def = new StructDef(manager, JObject.Parse(json));
            var element = new TestStructElement(def, manager);
            element.Initialize();
            return element;
        }

        private static void AssertMap(
            esper.defs.TES5.CTDAParam1Decider decider,
            IEnumerable<(CTDAFunctionParamType type, int index)> expected
        ) {
            foreach (var (type, index) in expected)
                Assert.AreEqual(index, decider.GetUnionIndex(type), type.ToString());
        }

        [Test]
        public void CTDAParamTypesUseEachGamesUnionOrder() {
            AssertMap(new esper.defs.TES4.CTDAParam1Decider(), tes4Params);
            AssertMap(new esper.defs.FO3.CTDAParam1Decider(), fo3Params);
            AssertMap(new esper.defs.FO4.Deciders.CTDAParam1Decider(), fo4Params);
        }

        [Test]
        public void FO4CurrentPackageQuestAliasKeepsPackageParameter() {
            var ctda = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Type', 'type': 'uint8' },
                    { 'name': 'Function', 'type': 'uint16' },
                    { 'name': 'Run On', 'type': 'uint32' }
                ]
            }");
            ctda.SetData("Type", (byte)0x02);
            ctda.SetData("Function", (ushort)161);
            ctda.SetData("Run On", (uint)5);

            var decider = new esper.defs.FO4.Deciders.CTDAParam1Decider();
            Assert.AreEqual(38, decider.Decide(ctda));
            ctda.SetData("Run On", (uint)0);
            Assert.AreEqual(9, decider.Decide(ctda));
        }

        [Test]
        public void LegacyCTDAParamsIgnoreTES5AliasFlags() {
            var ctdaJson = @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Type', 'type': 'uint8' },
                    { 'name': 'Function', 'type': 'uint16' }
                ]
            }";
            var tes4 = NewStruct(Games.TES4, ctdaJson);
            tes4.SetData("Type", (byte)0x02);
            tes4.SetData("Function", (ushort)1);
            Assert.AreEqual(
                10,
                new esper.defs.TES4.CTDAParam1Decider().Decide(tes4)
            );

            var fo3 = NewStruct(Games.FO3, ctdaJson);
            fo3.SetData("Type", (byte)0x02);
            fo3.SetData("Function", (ushort)1);
            Assert.AreEqual(
                14,
                new esper.defs.FO3.CTDAParam1Decider().Decide(fo3)
            );

            var missingFunction = NewStruct(Games.FO3, @"{
                'type': 'struct',
                'elements': [{ 'name': 'Type', 'type': 'uint8' }]
            }");
            Assert.AreEqual(
                0,
                new esper.defs.FO3.CTDAParam1Decider().Decide(missingFunction)
            );
        }

        [Test]
        public void NativeNumericDiscriminatorsDoNotRequireInt32Storage() {
            var vats = NewStruct(Games.SSE, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Parameter #1', 'type': 'uint32' }
                ]
            }");
            vats.SetData("Parameter #1", (uint)20);
            Assert.AreEqual(
                20,
                new esper.defs.TES5.CTDAParam2VATSValueParamDecider().Decide(vats)
            );
            Assert.AreEqual(
                20,
                new esper.defs.FO3.CTDAParam2VATSValueParam().Decide(vats)
            );

            var book = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [{ 'name': 'Flags', 'type': 'uint8' }]
            }");
            var bookDecider = new esper.defs.FO4.Deciders.BOOKTeachesDecider();
            book.SetData("Flags", (byte)0x14);
            Assert.AreEqual(2, bookDecider.Decide(book));
            book.SetData("Flags", (byte)0x11);
            Assert.AreEqual(1, bookDecider.Decide(book));
            book.SetData("Flags", (byte)0x04);
            Assert.AreEqual(
                1,
                new esper.defs.TES5.BOOKTeachesDecider().Decide(book)
            );
        }

        [Test]
        public void NAVIIslandPreservesZeroAndOneBranches() {
            var navi = NewStruct(Games.FO4, @"{
                'signature': 'NAVI',
                'type': 'struct',
                'elements': [
                    { 'name': 'Is Island', 'type': 'uint8' },
                    {
                        'name': 'Island Holder',
                        'type': 'struct',
                        'elements': []
                    }
                ]
            }");
            var holder = (Container)navi.GetElement("Island Holder");
            var decider = new esper.defs.TES5.NAVIIslandDataDecider();
            navi.SetData("Is Island", (byte)0);
            Assert.AreEqual(0, decider.Decide(holder));
            navi.SetData("Is Island", (byte)1);
            Assert.AreEqual(1, decider.Decide(holder));
        }

        [Test]
        public void FO4NVNMUsesParentWorldspaceFormId() {
            var nvnm = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Parent Worldspace', 'type': 'formId' }
                ]
            }");
            var decider = new esper.defs.FO4.Deciders.NVNMParentDecider();
            Assert.AreEqual(1, decider.Decide(nvnm));
            nvnm.SetData("Parent Worldspace", new FormId(null, 1));
            Assert.AreEqual(0, decider.Decide(nvnm));
        }

        [Test]
        public void LegacyGMSTBooleanUsesIntegerBranch() {
            var gmst = NewStruct(Games.FO3, @"{
                'type': 'struct',
                'elements': [{ 'name': 'EDID', 'type': 'string' }]
            }");
            var decider = new esper.defs.FO3.GMSTUnionDecider();
            gmst.SetData("EDID", "bTestSetting");
            Assert.AreEqual(1, decider.Decide(gmst));
            gmst.SetData("EDID", "fTestSetting");
            Assert.AreEqual(2, decider.Decide(gmst));
            gmst.SetData("EDID", "sTestSetting");
            Assert.AreEqual(0, decider.Decide(gmst));
        }

        [Test]
        public void TES4PGRRCounterUsesMatchingPointsConnectionCount() {
            var record = NewStruct(Games.TES4, @"{
                'type': 'struct',
                'elements': [
                    {
                        'name': 'Points',
                        'type': 'array',
                        'element': {
                            'name': 'Point',
                            'type': 'struct',
                            'elements': [
                                { 'name': 'Connections', 'type': 'uint8' }
                            ]
                        }
                    },
                    {
                        'name': 'Connections',
                        'type': 'array',
                        'element': { 'name': 'Point', 'type': 'int16' }
                    }
                ]
            }");
            var points = (ArrayElement)record.GetElement("Points");
            points.GetElement("[0]").SetData("Connections", (byte)2);
            foreach (byte count in new byte[] { 0, 3 }) {
                var point = (Container)points.arrayDef.elementDef.NewElement(points);
                point.Initialize();
                point.SetData("Connections", count);
            }

            var connectionsDef = (ArrayDef)record.def.childDefs[1];
            var currentRows = new ArrayElement(record, connectionsDef);
            var counter = new esper.defs.TES4.CalcPGRRSize(
                record.manager,
                JObject.Parse("{ 'type': 'CalcPGRRSize' }")
            );
            connectionsDef.elementDef.NewElement(currentRows);
            Assert.AreEqual(2, counter.GetCount(currentRows));
            connectionsDef.elementDef.NewElement(currentRows);
            Assert.AreEqual(0, counter.GetCount(currentRows));
            connectionsDef.elementDef.NewElement(currentRows);
            Assert.AreEqual(3, counter.GetCount(currentRows));
        }

        [Test]
        public void FO4REFRFlagsUseFO4BaseRecordGroups() {
            var expected = new Dictionary<string, int> {
                { "ACTI", 1 }, { "STAT", 1 }, { "SCOL", 1 }, { "TREE", 1 },
                { "CONT", 2 }, { "TERM", 2 }, { "DOOR", 3 }, { "LIGH", 4 },
                { "MSTT", 5 }, { "ADDN", 6 }, { "SCRL", 7 }, { "AMMO", 7 },
                { "ARMO", 7 }, { "BOOK", 7 }, { "INGR", 7 }, { "KEYM", 7 },
                { "MISC", 7 }, { "FURN", 7 }, { "WEAP", 7 }, { "ALCH", 7 }
            };
            foreach (var (signature, index) in expected)
                Assert.AreEqual(
                    index,
                    esper.defs.FO4.Deciders.REFRRecordFlagsDecider.UnionIndex(
                        signature
                    ),
                    signature
                );
            Assert.AreEqual(
                0,
                esper.defs.FO4.Deciders.REFRRecordFlagsDecider.UnionIndex("SLGM")
            );
        }

        [Test]
        public void SessionsRegisterGameSpecificParityImplementations() {
            var tes4 = RecordDefCoverageTests.GetSession("TES4");
            Assert.IsInstanceOf<esper.defs.TES4.CTDAParam1Decider>(
                tes4.definitionManager.GetDecider("CTDAParam1Decider")
            );
            var pgrd = tes4.definitionManager.GetRecordDef(
                Signature.FromString("PGRD")
            );
            Assert.IsTrue(Walk(pgrd).OfType<ArrayDef>().Any(a =>
                a.counterDef is esper.defs.TES4.CalcPGRRSize
            ));
            var mgef = tes4.definitionManager.GetRecordDef(
                Signature.FromString("MGEF")
            );
            Assert.IsTrue(Walk(mgef).Any(d =>
                d is esper.defs.TES4.MgefCodeDef
            ));
            Assert.IsFalse(Walk(mgef).OfType<ArrayDef>().Any(a =>
                a.elementDef == null
            ));

            var fnv = RecordDefCoverageTests.GetSession("FNV");
            Assert.IsInstanceOf<esper.defs.FO3.CTDAParam1Decider>(
                fnv.definitionManager.GetDecider("CTDAParam1Decider")
            );
            var fo4 = RecordDefCoverageTests.GetSession("FO4");
            Assert.IsInstanceOf<esper.defs.FO4.Deciders.CTDAParam1Decider>(
                fo4.definitionManager.GetDecider("CTDAParam1Decider")
            );
        }

        private static IEnumerable<ElementDef> Walk(ElementDef def) {
            yield return def;
            foreach (var child in def.childDefs ?? Enumerable.Empty<ElementDef>())
                foreach (var nested in Walk(child))
                    yield return nested;
        }
    }
}
