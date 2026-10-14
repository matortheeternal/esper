using esper;
using esper.defs;
using esper.defs.TES4;
using esper.defs.TES5;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;

namespace Tests.defs {
    [TestFixture]
    public class SmallFormatCoverageTests {
        private static DefinitionManager SseManager() {
            return RecordDefCoverageTests.GetSession("SSE")
                .definitionManager;
        }

        private static JObject Named(string name) {
            return JObject.Parse("{'name':'" + name + "'}");
        }

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

        [Test]
        public void EdgeFormats() {
            var mgr = SseManager();
            var e0 = new Edge0Format(mgr, Named("E0"));
            var e1 = new Edge1Format(mgr, Named("E1"));
            var e2 = new Edge2Format(mgr, Named("E2"));
            Assert.IsTrue(e0.customSortKey);
            Assert.AreEqual("5", e0.DataToValue(null, 5));
            int none1 = e0.ValueToData(null, "");
            int none2 = e1.ValueToData(null, "None");
            Assert.AreEqual(-1, none1);
            Assert.AreEqual(-1, none2);
            long twelve = e2.ValueToData(null, "12");
            Assert.AreEqual(12L, twelve);
            Assert.AreEqual("000000000005", e0.GetSortKey(null, (UInt16)5));
        }

        [Test]
        public void VertexFormats() {
            var mgr = SseManager();
            var v0 = new Vertex0Format(mgr, Named("V0"));
            var v1 = new Vertex1Format(mgr, Named("V1"));
            var v2 = new Vertex2Format(mgr, Named("V2"));
            Assert.IsTrue(v0.isNumeric);
            Assert.IsTrue(v0.customSortKey);
            Assert.AreEqual("7", v0.DataToValue(null, 7));
            long seven = v1.ValueToData(null, "7");
            Assert.AreEqual(7L, seven);
            Assert.AreEqual("0007", v2.GetSortKey(null, (UInt16)7));
        }

        [Test]
        public void AtxtPositionFormat() {
            var mgr = SseManager();
            var fmt = new AtxtPositionFormat(mgr, Named("A"));
            Assert.IsTrue(fmt.customSortKey);
            Assert.AreEqual("18 -> 1:1", fmt.DataToValue(null, (UInt16)18));
            long back = fmt.ValueToData(null, "18");
            Assert.AreEqual(18L, back);
            Assert.AreEqual("0101", fmt.GetSortKey(null, (UInt16)18));
        }

        [Test]
        public void RotationFactorFormat() {
            var mgr = SseManager();
            var fmt = new RotationFactorFormat(mgr, Named("R"));
            Assert.IsTrue(fmt.isNumeric);
            Assert.AreEqual("180.0000", fmt.DataToValue(null, (float)Math.PI));
            double back = fmt.ValueToData(null, "180");
            Assert.AreEqual(Math.PI, back, 1e-6);
        }

        [Test]
        public void CloudSpeedFormat() {
            var mgr = SseManager();
            var fmt = new CloudSpeedFormat(mgr, Named("C"));
            Assert.AreEqual("0.0000", fmt.DataToValue(null, 127));
            double back = fmt.ValueToData(null, "0");
            Assert.AreEqual(127.0, back);
        }

        [Test]
        public void HideFFFFFormat() {
            var mgr = SseManager();
            var fmt = new HideFFFF_Format(mgr, Named("H"));
            Assert.AreEqual("None", fmt.DataToValue(null, 0xFFFF));
            Assert.AreEqual("42", fmt.DataToValue(null, 42));
            int none = fmt.ValueToData(null, "None");
            Assert.AreEqual(0xFFFF, none);
            long plain = fmt.ValueToData(null, "42");
            Assert.AreEqual(42L, plain);
        }

        [Test]
        public void TimeFormat() {
            var mgr = SseManager();
            var fmt = new TimeFormat(mgr, Named("T"));
            Assert.AreEqual("12:00", fmt.DataToValue(null, 72L));
            Assert.AreEqual("150", fmt.DataToValue(null, 150L));
            byte back = fmt.ValueToData(null, "12:00");
            Assert.AreEqual((byte)72, back);
        }

        [Test]
        public void EventFormat() {
            var mgr = SseManager();
            var fmt = new EventFormat(mgr, Named("E"));
            Assert.AreEqual("GetIsID:None", fmt.DataToValue(null, 0));
            Assert.AreEqual(
                "GetValue:Form", fmt.DataToValue(null, (12614 << 16) | 2)
            );
            int noColon = fmt.ValueToData(null, "NoColonHere");
            Assert.AreEqual(0, noColon);
            // The parser drops the character before ':', so a valid
            // round-trip input carries one extra character there.
            long parsed = fmt.ValueToData(null, "GetIsIDX:None");
            Assert.AreEqual(0L, parsed);
        }

        [Test]
        public void NextObjectIDFormat() {
            var mgr = SseManager();
            var fmt = new NextObjectIDFormat(mgr, Named("N"));
            Assert.IsTrue(fmt.isNumeric);
            Assert.AreEqual("000ABC", fmt.DataToValue(null, (UInt32)0xABC));
            int empty = fmt.ValueToData(null, "");
            Assert.AreEqual(2048, empty);
            long hex = fmt.ValueToData(null, "10");
            Assert.AreEqual(16L, hex);
            long capped = fmt.ValueToData(null, "100000");
            Assert.AreEqual(2048L, capped);
        }

        private static PluginFile LoadOblivion() {
            return Fixture.Master(Games.TES4, "Oblivion.esm");
        }

        [Test]
        public void NextObjectIDQuestionMarkUsesFileHighObjectId() {
            var plugin = LoadOblivion();
            if (plugin == null) Assert.Ignore("Oblivion.esm missing.");
            var found = FindFormatElement(
                plugin.header, typeof(NextObjectIDFormat)
            );
            if (found == null) Assert.Ignore("No NextObjectID element.");
            var fmt = (NextObjectIDFormat)found.valueDef.formatDef;
            var expected = Math.Min(
                (long)found.file.GetHighObjectID() + 1, 2048L
            );
            long actual = fmt.ValueToData(found, "?");
            Assert.AreEqual(expected, actual);
        }

        private static ValueElement FindFormatElement(Element root, Type t) {
            if (root is ValueElement v && v.valueDef?.formatDef?.GetType() == t)
                return v;
            if (root is Container c)
                foreach (var child in c.elements) {
                    var hit = FindFormatElement(child, t);
                    if (hit != null) return hit;
                }
            return null;
        }

        [Test]
        public void CTDAParamStringFormats() {
            var mgr = SseManager();
            var bare = new CTDAParamStringFormat(mgr, Named("B"));
            Assert.Throws<NotImplementedException>(
                () => bare.GetParamElement(null)
            );
            var c1 = new CTDAParam1StringFormat(mgr, Named("C1"));
            var c2 = new CTDAParam2StringFormat(mgr, Named("C2"));
            Assert.IsNull(c1.GetParamElement(null));
            Assert.IsNull(c2.GetParamElement(null));
            Assert.AreEqual("", c1.DataToValue(null, 5));
            Assert.AreEqual(0, c2.ValueToData(null, "x"));
        }

        [Test]
        public void EmptyDef() {
            var mgr = SseManager();
            var def = new EmptyDef(mgr, Named("E"));
            var element = new ValueElement(null, def);
            Assert.AreEqual(XEDefType.dtEmpty, def.valueDefType);
            Assert.AreEqual(0, def.size);
            Assert.AreEqual("", def.GetValue(element));
            def.SetValue(element, "x");
            Assert.IsNull(def.DefaultData());
            Assert.IsNull(def.ReadData(null, null));
            Assert.AreEqual("", def.GetSortKey(element));
            def.WriteData(element, null);
            Assert.AreEqual("empty", (string)def.ToJObject()["type"]);
            var slim = def.ToJObject(false);
            Assert.AreEqual(1, slim.Count);
            Assert.IsNotNull(slim["id"]);
        }

        [Test]
        public void Int0Def() {
            var mgr = SseManager();
            var def = new Int0Def(mgr, Named("I"));
            var element = new ValueElement(null, def);
            Assert.IsTrue(def.isNumeric);
            Assert.AreEqual(0, def.size);
            Assert.IsNull(def.ReadData(null, null));
            Assert.IsNull(def.DefaultData());
            Assert.AreEqual("", def.GetValue(element));
            def.SetValue(element, "5");
            Assert.IsNull(element._data);
            Assert.AreEqual("", def.GetSortKey(element));
            def.WriteData(element, null);
            Assert.AreEqual("int0", (string)def.ToJObject()["type"]);
        }

        [Test]
        public void CounterDefBaseThrows() {
            var mgr = SseManager();
            var def = new CounterDef(mgr, Named("C"));
            Assert.IsFalse(def.canSetCount);
            Assert.Throws<NotImplementedException>(() => def.GetCount(null));
            Assert.Throws<NotImplementedException>(
                () => def.SetCount(null, 1)
            );
            var offset = new OffsetDataColsCounter(mgr, Named("O"));
            Assert.IsFalse(offset.canSetCount);
        }

        private static Container OffsetChild(
            string minX, string maxX, bool withBounds = true,
            bool withNam0 = true, bool withNam9 = true
        ) {
            // Omitted NAM structs stay truly absent: Initialize only
            // materializes elements present in the def, so their GetData
            // reads null instead of a default 0.
            var inner = "";
            if (withNam0) inner += @"{'name':'NAM0','type':'struct',
                'elements':[{'name':'X','type':'float'}]},";
            if (withNam9) inner += @"{'name':'NAM9','type':'struct',
                'elements':[{'name':'X','type':'float'}]},";
            var members = withBounds
                ? @"{'name':'Object Bounds','type':'struct','elements':["
                    + inner.TrimEnd(',') + "]},"
                : "";
            var json = @"{'name':'R','type':'struct','elements':["
                + members + "{'name':'Child','type':'struct','elements':[]}]}";
            var root = NewStruct(Games.SSE, json);
            if (withBounds) {
                if (minX != null)
                    root.SetValue(@"Object Bounds\NAM0\X", minX);
                if (maxX != null)
                    root.SetValue(@"Object Bounds\NAM9\X", maxX);
            }
            return root.GetElement("Child") as Container;
        }

        [Test]
        public void OffsetDataColsCounter() {
            var mgr = SseManager();
            var counter = new OffsetDataColsCounter(mgr, Named("O"));
            Assert.AreEqual(6u, counter.GetCount(OffsetChild("5.0", "10.0")));
            // minX <= 0 skips truncation, so max stays huge and the
            // result is not reset to 1: 10 - 0 + 1.
            Assert.AreEqual(11u, counter.GetCount(OffsetChild("0.0", "10.0")));
            Assert.AreEqual(0u, counter.GetCount(OffsetChild("5.0", "0.5")));
            // maxX beyond int range resets the result to 1.
            Assert.AreEqual(
                3000000000u, counter.GetCount(OffsetChild("5.0", "3000000000"))
            );
            Assert.AreEqual(
                5u, counter.GetCount(OffsetChild("5.0", null, true, true, false))
            );
            Assert.AreEqual(
                0u, counter.GetCount(OffsetChild(null, "10.0", true, false, true))
            );
            Assert.AreEqual(
                0u, counter.GetCount(OffsetChild("5.0", "10.0", false))
            );
            Assert.Throws<NotImplementedException>(
                () => counter.SetCount(OffsetChild("5.0", "10.0"), 2)
            );
        }

        [Test]
        public void Int0DefDeclaresIntegerType() {
            var def = new Int0Def(SseManager(), Named("I"));
            Assert.AreEqual(XEDefType.dtInteger, def.valueDefType);
            Assert.IsTrue(def.isNumeric);
        }

        private sealed class EdgeProbe : EdgeFormat {
            internal EdgeProbe(DefinitionManager manager, JObject src)
                : base(manager, src) { }
            public int BaseEdge => edge;
        }

        private sealed class VertexProbe : VertexFormat {
            internal VertexProbe(DefinitionManager manager, JObject src)
                : base(manager, src) { }
            public int BaseVertex => vertex;
        }

        [Test]
        public void EdgeAndVertexBasePropertiesDefaultToZero() {
            var manager = SseManager();
            Assert.AreEqual(0, new EdgeProbe(manager, new JObject()).BaseEdge);
            Assert.AreEqual(0, new VertexProbe(manager, new JObject()).BaseVertex);
        }

        [Test]
        public void Tes4MiscActorValueDeciderMatchesRecordFlags() {
            var plugin = LoadOblivion();
            if (plugin == null) Assert.Ignore("Oblivion.esm missing.");
            var decider = new MISCActorValueDecider();
            var checkedCount = 0;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "MISC") continue;
                var expected = (rec.header.flags & 0xC0) == 0xC0 ? 1 : 0;
                Assert.AreEqual(expected, decider.Decide(rec));
                if (++checkedCount >= 50) break;
            }
            Assert.Greater(checkedCount, 0, "No MISC records found.");
        }
    }
}
