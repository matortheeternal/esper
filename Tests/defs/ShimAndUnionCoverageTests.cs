using esper;
using esper.defs;
using esper.defs.TES5;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tests.defs {
    // Covers the remaining thin branches found by the coverage report:
    // game-specific CTDA union default arms and Param2 overrides, the
    // FO4 BOOK/NVNM decider arms, every FromTES5 shim constructor, and
    // TintLayerFormat driven by real Skyrim.esm NPC_ tint layers.
    [TestFixture]
    public class ShimAndUnionCoverageTests {
        private class TestStructElement : StructElement {
            private readonly DefinitionManager definitionManager;

            public override DefinitionManager manager => definitionManager;

            public TestStructElement(
                StructDef def, DefinitionManager definitionManager
            ) : base(null, def) {
                this.definitionManager = definitionManager;
            }
        }

        private static StructElement NewStruct(Game game, string json) {
            var manager = Synthetic.GetSession(game).definitionManager;
            var def = new StructDef(manager, JObject.Parse(json));
            var element = new TestStructElement(def, manager);
            element.Initialize();
            return element;
        }

        private static CTDAFunction NewFunction(
            Game game, string paramType1, string paramType2
        ) {
            var manager = Synthetic.GetSession(game).definitionManager;
            return new CTDAFunction(manager, JObject.Parse(
                "{'index':1,'name':'GetTest'," +
                $"'paramType1':'{paramType1}'," +
                $"'paramType2':'{paramType2}'}}"
            ));
        }

        [Test]
        public void TES4UnionDefaultBranchIsZero() {
            var decider = new esper.defs.TES4.CTDAParam1Decider();
            Assert.AreEqual(
                0, decider.GetUnionIndex((CTDAFunctionParamType)Enum.ToObject(typeof(CTDAFunctionParamType), 999))
            );
            Assert.AreEqual(
                0, decider.GetUnionIndex(CTDAFunctionParamType.ptFloat)
            );
        }

        [Test]
        public void FO3UnionDefaultBranchIsZero() {
            var decider = new esper.defs.FO3.CTDAParam1Decider();
            Assert.AreEqual(
                0, decider.GetUnionIndex((CTDAFunctionParamType)Enum.ToObject(typeof(CTDAFunctionParamType), 999))
            );
            Assert.AreEqual(
                0, decider.GetUnionIndex(CTDAFunctionParamType.ptFloat)
            );
        }

        [Test]
        public void FO4UnionDefaultBranchIsZero() {
            Assert.AreEqual(
                0, esper.defs.FO4.Deciders.CTDAParam1Decider.UnionIndex(
                    (CTDAFunctionParamType)Enum.ToObject(typeof(CTDAFunctionParamType), 999)
                )
            );
        }

        [Test]
        public void FO4Param2DelegatesToParam1Table() {
            var decider = new esper.defs.FO4.Deciders.CTDAParam2Decider();
            Assert.AreEqual(
                5, decider.GetUnionIndex(CTDAFunctionParamType.ptActor)
            );
            Assert.AreEqual(
                56, decider.GetUnionIndex(CTDAFunctionParamType.ptWorldspace)
            );
        }

        [Test]
        public void TES4Param2ReadsSecondParamType() {
            var fn = NewFunction(Games.TES4, "ptActor", "ptGlobal");
            Assert.AreEqual(
                CTDAFunctionParamType.ptActor,
                new esper.defs.TES4.CTDAParam1Decider().GetParamType(fn)
            );
            Assert.AreEqual(
                CTDAFunctionParamType.ptGlobal,
                new esper.defs.TES4.CTDAParam2Decider().GetParamType(fn)
            );
        }

        [Test]
        public void FO3Param2ReadsSecondParamType() {
            var fn = NewFunction(Games.FO3, "ptActor", "ptGlobal");
            Assert.AreEqual(
                CTDAFunctionParamType.ptActor,
                new esper.defs.FO3.CTDAParam1Decider().GetParamType(fn)
            );
            Assert.AreEqual(
                CTDAFunctionParamType.ptGlobal,
                new esper.defs.FO3.CTDAParam2Decider().GetParamType(fn)
            );
        }

        [Test]
        public void FO4BookTeachesAllArms() {
            var book = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    {
                        'name': 'Flags', 'type': 'uint32',
                        'required': true
                    }
                ]
            }");
            var decider = new esper.defs.FO4.Deciders.BOOKTeachesDecider();
            book.SetData("Flags", (UInt32)0x01);
            Assert.AreEqual(1, decider.Decide(book));
            book.SetData("Flags", (UInt32)0x04);
            Assert.AreEqual(2, decider.Decide(book));
            book.SetData("Flags", (UInt32)0x10);
            Assert.AreEqual(3, decider.Decide(book));
            book.SetData("Flags", (UInt32)0x00);
            Assert.AreEqual(0, decider.Decide(book));
            book.SetData("Flags", (UInt32)0x05);
            Assert.AreEqual(1, decider.Decide(book));
            var noFlags = NewStruct(Games.FO4, @"{
                'type': 'struct', 'elements': []
            }");
            Assert.AreEqual(0, decider.Decide(noFlags));
            Assert.AreEqual(0, decider.Decide(null));
            Assert.AreEqual(
                0, new esper.defs.TES5.BOOKTeachesDecider().Decide(null)
            );
        }

        [Test]
        public void FO4CurrentPackageFallsBackWithoutMatchingRunOn() {
            var noRunOn = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Type', 'type': 'uint8' },
                    { 'name': 'Function', 'type': 'uint16' }
                ]
            }");
            noRunOn.SetData("Type", (byte)0x02);
            noRunOn.SetData("Function", (ushort)161);
            var decider = new esper.defs.FO4.Deciders.CTDAParam1Decider();
            // Name matches GetIsCurrentPackage but Run On is absent, so
            // the package-quest-alias shortcut is skipped like Run On = 0.
            Assert.AreEqual(9, decider.Decide(noRunOn));

            var otherFunction = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Type', 'type': 'uint8' },
                    { 'name': 'Function', 'type': 'uint16' },
                    { 'name': 'Run On', 'type': 'uint32' }
                ]
            }");
            otherFunction.SetData("Type", (byte)0x02);
            otherFunction.SetData("Function", (ushort)1);
            otherFunction.SetData("Run On", (uint)5);
            var zeroRunOn = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Type', 'type': 'uint8' },
                    { 'name': 'Function', 'type': 'uint16' },
                    { 'name': 'Run On', 'type': 'uint32' }
                ]
            }");
            zeroRunOn.SetData("Type", (byte)0x02);
            zeroRunOn.SetData("Function", (ushort)1);
            zeroRunOn.SetData("Run On", (uint)0);
            // A non-matching function name takes the base path even with
            // the Run On value the shortcut checks for.
            Assert.AreEqual(
                decider.Decide(zeroRunOn), decider.Decide(otherFunction)
            );

            // Name and Run On match, but with no Type element the
            // alias/packdata flag check sees zero flags, so the base
            // path keeps the plain ptPackage parameter (index 38 here).
            var noType = NewStruct(Games.FO4, @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Function', 'type': 'uint16' },
                    { 'name': 'Run On', 'type': 'uint32' }
                ]
            }");
            noType.SetData("Function", (ushort)161);
            noType.SetData("Run On", (uint)5);
            Assert.AreEqual(38, decider.Decide(noType));
        }

        [Test]
        public void TES5AliasAndPackdataRemap() {
            // GetDistance takes an ObjectReference first parameter, so it
            // exercises the alias/packdata flag remap in the TES5 base.
            const string json = @"{
                'type': 'struct',
                'elements': [
                    { 'name': 'Type', 'type': 'uint8' },
                    { 'name': 'Function', 'type': 'uint16' }
                ]
            }";
            var decider = new esper.defs.TES5.CTDAParam1Decider();
            var alias = NewStruct(Games.SSE, json);
            alias.SetData("Type", (byte)0x02);
            alias.SetData("Function", (ushort)1);
            Assert.AreEqual(
                (int)CTDAFunctionParamType.ptAlias + 1,
                decider.Decide(alias)
            );
            var packdata = NewStruct(Games.SSE, json);
            packdata.SetData("Type", (byte)0x08);
            packdata.SetData("Function", (ushort)1);
            Assert.AreEqual(
                (int)CTDAFunctionParamType.ptPackdata + 1,
                decider.Decide(packdata)
            );
            var plain = NewStruct(Games.SSE, json);
            plain.SetData("Type", (byte)0x00);
            plain.SetData("Function", (ushort)1);
            Assert.AreEqual(
                (int)CTDAFunctionParamType.ptObjectReference + 1,
                decider.Decide(plain)
            );
        }

        [Test]
        public void TES5VATSDeciderNullIsZero() {
            Assert.AreEqual(
                0,
                new esper.defs.TES5.CTDAParam2VATSValueParamDecider()
                    .Decide(null)
            );
        }

        [Test]
        public void FO4RefrUnionIndexCoversAllArms() {
            var index = esper.defs.FO4.Deciders.REFRRecordFlagsDecider
                .UnionIndex;
            foreach (var sig in new[] { "ACTI", "STAT", "SCOL", "TREE" })
                Assert.AreEqual(1, index(sig), sig);
            foreach (var sig in new[] { "CONT", "TERM" })
                Assert.AreEqual(2, index(sig), sig);
            Assert.AreEqual(3, index("DOOR"));
            Assert.AreEqual(4, index("LIGH"));
            Assert.AreEqual(5, index("MSTT"));
            Assert.AreEqual(6, index("ADDN"));
            foreach (var sig in new[] {
                "SCRL", "AMMO", "ARMO", "BOOK", "INGR", "KEYM",
                "MISC", "FURN", "WEAP", "ALCH"
            })
                Assert.AreEqual(7, index(sig), sig);
            Assert.AreEqual(0, index("NPC_"));
            Assert.AreEqual(0, index(null));
        }

        [Test]
        public void FO4RefrFlagsDecideMatchesBaseType() {
            var plugin = Fixture.Master(Games.FO4, "Fallout4.esm");
            var decider =
                new esper.defs.FO4.Deciders.REFRRecordFlagsDecider();
            int seen = 0, nonzero = 0;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "REFR") continue;
                if (seen >= 200) break;
                seen++;
                var header = rec.GetElement("Record Header") as Container;
                Assert.IsNotNull(header, "REFR has no Record Header");
                var baseRec = header.GetElement(@"..\@NAME") as MainRecord;
                var sig = baseRec?.signature.ToString();
                var branch = decider.Decide(header);
                Assert.AreEqual(
                    esper.defs.FO4.Deciders.REFRRecordFlagsDecider
                        .UnionIndex(sig),
                    branch,
                    $"REFR base type {sig ?? "<none>"}"
                );
                if (branch != 0) nonzero++;
            }
            Assert.Greater(seen, 0, "Fallout4.esm has no REFR records");
            Assert.Greater(
                nonzero, 0, "No nonzero REFR flag branch exercised"
            );
        }

        [Test]
        public void FO4NVNMNullContainerIsZero() {
            var decider = new esper.defs.FO4.Deciders.NVNMParentDecider();
            Assert.AreEqual(0, decider.Decide(null));
        }

        [Test]
        public void AllGameNamespaceShimsConstruct() {
            // SF ships only the TES4 header record, so its manager cannot
            // resolve the TES5 defs some shim base constructors look up.
            // The shims only chain to those base constructors, so build
            // them with the SSE manager instead.
            var managers = new Dictionary<string, DefinitionManager> {
                ["SF"] = Synthetic.GetSession(Games.SSE).definitionManager,
                ["FO4"] = Synthetic.GetSession(Games.FO4).definitionManager,
            };
            var asm = typeof(Def).Assembly;
            var argTypes = new[] {
                typeof(DefinitionManager), typeof(JObject)
            };
            int count = 0;
            foreach (var type in asm.GetTypes()) {
                if (type.IsAbstract || type.IsInterface) continue;
                var ns = type.Namespace ?? "";
                string game = null;
                if (ns == "esper.defs.SF" || ns.StartsWith("esper.defs.SF."))
                    game = "SF";
                else if (
                    ns == "esper.defs.FO4" ||
                    ns.StartsWith("esper.defs.FO4.")
                )
                    game = "FO4";
                if (game == null) continue;
                var ctor = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic,
                    null, argTypes, null
                );
                if (ctor == null) continue;
                // ActorValueFormat shims chain to the TES5 base
                // constructor, which resolves ActorValueEnum from the raw
                // def JSON. Raw defs are stripped after session build, so
                // post-build construction NREs; production only builds
                // defs during session construction, where it works.
                if (typeof(ActorValueFormat).IsAssignableFrom(type))
                    continue;
                var instance = ctor.Invoke(
                    new object[] { managers[game], new JObject() }
                );
                Assert.IsNotNull(instance, type.FullName);
                count++;
            }
            Assert.Greater(count, 40, "Expected dozens of SF/FO4 classes");
        }

        private static void FindStringParamElements(
            Container container, List<ValueElement> hits
        ) {
            foreach (var element in container.elements) {
                if (
                    element is ValueElement value &&
                    value.def is ValueDef valueDef &&
                    valueDef.formatDef is CTDAParamStringFormat
                ) {
                    hits.Add(value);
                    continue;
                }
                if (element is Container child)
                    FindStringParamElements(child, hits);
            }
        }

        [Test]
        public void CTDAParamStringFormatsResolveThroughCIS() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var skyrim = Fixture.Master(Games.SSE, "Skyrim.esm");
            var hits = new List<ValueElement>();
            foreach (var rec in skyrim.records) {
                if (rec.signature.ToString() != "INFO") continue;
                FindStringParamElements(rec, hits);
                if (hits.Count >= 4) break;
            }
            Assert.IsNotEmpty(
                hits, "Skyrim.esm has no CTDA string param element"
            );
            var manager = session.definitionManager;
            foreach (var element in hits) {
                var format =
                    (CTDAParamStringFormat)((ValueDef)element.def)
                        .formatDef;
                var cis = format is CTDAParam1StringFormat
                    ? @"..\..\CIS1"
                    : @"..\..\CIS2";
                var param =
                    element.GetElement(cis) as ValueElement;
                var expected = param?.value ?? "";
                Assert.AreEqual(
                    expected, format.DataToValue(element, element.data)
                );
                Assert.AreEqual(0, format.ValueToData(element, "1").ToInt64());
            }
            Assert.Throws<NotImplementedException>(() =>
                new CTDAParamStringFormat(manager, new JObject())
                    .GetParamElement(null)
            );
        }

        private static ValueElement FindTintElement(Container container) {
            foreach (var element in container.elements) {
                if (
                    element is ValueElement value &&
                    value.def is ValueDef valueDef &&
                    valueDef.formatDef is TintLayerFormat
                )
                    return value;
                if (element is Container child) {
                    var found = FindTintElement(child);
                    if (found != null) return found;
                }
            }
            return null;
        }

        [Test]
        public void TintLayerFormatDrivenByRealNpc() {
            var session = RecordDefCoverageTests.GetSession("SSE");
            var skyrim = Fixture.Master(Games.SSE, "Skyrim.esm");
            // Localized race FULL names need the string tables; without
            // them the missing-index fallback below cannot render.
            var stringFiles = Fixture.StringFiles(Games.SSE, "Skyrim.esm");
            var haveStrings = stringFiles.Count > 0
                && stringFiles.All(File.Exists);
            if (haveStrings)
                session.resourceManager.LoadPluginStrings(
                    skyrim, stringFiles
                );
            var hits = new List<ValueElement>();
            foreach (var rec in skyrim.records) {
                if (rec.signature.ToString() != "NPC_") continue;
                var found = FindTintElement(rec);
                if (found != null) hits.Add(found);
                if (hits.Count >= 8) break;
            }
            Assert.IsNotEmpty(
                hits, "Skyrim.esm has no NPC_ tint layer element"
            );
            var first = hits[0];
            var firstFormat =
                (TintLayerFormat)((ValueDef)first.def).formatDef;
            var index = Convert.ToInt32(first.data?.ToInt64() ?? 0);
            var value = firstFormat.DataToValue(first, first.data);
            Assert.IsTrue(
                value.StartsWith(index.ToString()),
                $"Unexpected tint value '{value}'"
            );
            // A second call for the same race/gender hits the cache entry
            // stored by the first call.
            Assert.AreEqual(
                value, firstFormat.DataToValue(first, first.data)
            );
            // An index the race has no entry for falls back to the
            // not-found message (needs string tables for race.name).
            if (haveStrings) {
                var missing = firstFormat.DataToValue(first, 999999);
                Assert.IsTrue(
                    missing.StartsWith("999999"),
                    $"Unexpected missing tint value '{missing}'"
                );
            }
            foreach (var element in hits.Skip(1)) {
                var format = (TintLayerFormat)((ValueDef)element.def)
                    .formatDef;
                var elementIndex = Convert.ToInt32(element.data?.ToInt64() ?? 0);
                var elementValue = format.DataToValue(
                    element, element.data
                );
                Assert.IsTrue(
                    elementValue.StartsWith(elementIndex.ToString()),
                    $"Unexpected tint value '{elementValue}'"
                );
            }
            var manager = session.definitionManager;
            var bare = new TintLayerFormat(manager, new JObject());
            Assert.IsTrue(bare.isNumeric);
            Assert.AreEqual(5, bare.ValueToData(null, "5").ToInt64());
            // Null container: no actor, race, or tint entries to resolve.
            Assert.AreEqual("5", bare.DataToValue(null, 5));
        }
    }
}
