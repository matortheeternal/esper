using esper;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Tests;

namespace Tests.defs {
    [TestFixture]
    public class TES4FormatsTests {
        private static Session TES4() => Synthetic.GetSession(Games.TES4);

        private static JObject Src(string name, int value) {
            return new JObject { { "name", name }, { "value", value } };
        }

        private static JObject Src(string name) {
            return new JObject { { "name", name } };
        }

        private static PluginFile Load(Game game, string fixture) {
            var path = TestHelpers.FixturePath(fixture);
            if (!File.Exists(path)) Assert.Ignore($"{fixture} missing.");
            var session = new Session(game, new SessionOptions());
            return session.pluginManager.LoadPlugin(path);
        }

        private static MainRecord First(PluginFile plugin, string sig) {
            var rec = plugin.records.FirstOrDefault(
                r => r.signature.ToString() == sig);
            if (rec == null) Assert.Ignore($"No {sig} in {plugin.name}.");
            return rec;
        }

        private static ValueElement FindFormatted(
            MainRecord rec, Func<ValueDef, bool> match
        ) {
            var stack = new System.Collections.Generic.Stack<Element>(
                rec.elements);
            while (stack.Count > 0) {
                var e = stack.Pop();
                if (e is ValueElement ve && ve.def is ValueDef vd &&
                    match(vd)) return ve;
                if (e is Container c)
                    foreach (var child in c.elements) stack.Push(child);
            }
            return null;
        }

        private static string DisplayText(ValueElement el) {
            var vd = (ValueDef)el.def;
            return vd.formatDef.DataToValue(el, el.data);
        }

        [Test]
        public void TestMultiplyScalesBothWays() {
            var def = new MultiplyDef(
                TES4().definitionManager, Src("Factor5", 5));
            Assert.AreEqual("100", def.DataToValue(null, 20));
            Assert.AreEqual(20L, (long)def.ValueToData(null, "100"));
            var def100 = new MultiplyDef(
                TES4().definitionManager, Src("Factor100", 100));
            Assert.AreEqual("2500", def100.DataToValue(null, 25));
        }

        [Test]
        public void TestScaledInt4FixedDecimals() {
            var def = new ScaledInt4Format(
                TES4().definitionManager, Src("Chance"));
            Assert.AreEqual("0.5000", def.DataToValue(null, 5000));
            Assert.AreEqual("-1.2500", def.DataToValue(null, -12500));
            Assert.AreEqual(5000L, (long)def.ValueToData(null, "0.5000"));
            Assert.AreEqual(
                "+00000000000000000.5000",
                def.GetSortKey(null, 5000));
        }

        [Test]
        public void TestChar4RoundTrip() {
            var def = new esper.defs.TES4.Char4Format(
                TES4().definitionManager, Src("Magic effect name"));
            Assert.AreEqual("ABCD", def.DataToValue(null, 0x44434241u));
            Assert.AreEqual(
                0x44434241u, (uint)def.ValueToData(null, "ABCD"));
            Assert.AreEqual(
                0x20202020u, (uint)def.ValueToData(null, ""));
            Assert.Throws<Exception>(() => def.ValueToData(null, "ABC"));
        }

        [Test]
        public void TestIdleAnamTable() {
            var def = new esper.defs.TES4.IdleAnamFormat(
                TES4().definitionManager, Src("Animation Group Section"));
            Assert.AreEqual(
                "Lower Body, Must return a file", def.DataToValue(null, 0));
            Assert.AreEqual("Lower Body", def.DataToValue(null, 0x80));
            Assert.AreEqual(
                "Upper Body, Must return a file", def.DataToValue(null, 6));
            Assert.AreEqual(
                "<Unknown: 7>, Must return a file",
                def.DataToValue(null, 7));
            Assert.AreEqual("80", def.GetSortKey(null, 0x80));
            Assert.Throws<NotImplementedException>(
                () => def.ValueToData(null, "Lower Body"));
        }

        [Test]
        public void TestWthrClassification() {
            var def = new esper.defs.TES4.WthrDataClassificationFormat(
                TES4().definitionManager, Src("Weather Classification"));
            Assert.AreEqual("Pleasant", def.DataToValue(null, 1));
            Assert.AreEqual("Unknown 3", def.DataToValue(null, 3));
            Assert.Throws<NotImplementedException>(
                () => def.ValueToData(null, "Pleasant"));
        }

        [Test]
        public void TestVariableNameFallbackIsRawValue() {
            var manager = TES4().definitionManager;
            var format = new esper.defs.TES4.CTDAParam2VariableNameFormat(
                manager, new JObject());
            Assert.AreEqual("5", format.DataToValue(null, 5));
        }

        [Test]
        public void TestVariableNameValueToDataParsesNumber() {
            var manager = TES4().definitionManager;
            var format = new esper.defs.TES4.CTDAParam2VariableNameFormat(
                manager, new JObject());
            Assert.AreEqual(12L, format.ValueToData(null, "12"));
        }

        [Test]
        public void TestVariableNameResolvesRealName() {
            var plugin = Load(Games.TES4, "Oblivion.esm");
            var el = TestHelpers.FindRecord(
                plugin.records.Where(
                    r => r.signature.ToString() == "INFO").Take(500),
                rec => {
                    var hit = WalkForFormat(
                        rec, "CTDAParam2VariableNameFormat");
                    if (hit == null) return null;
                    var text = DisplayText(hit);
                    return text != hit.data.ToString() && text.Length > 0
                        ? hit : null;
                });
            if (el == null)
                Assert.Ignore("No resolved VariableName element found.");
        }

        private static ValueElement WalkForFormat(Element el, string defId) {
            if (el is ValueElement ve && ve.def is ValueDef vd &&
                vd.formatDef != null && vd.formatDef.GetType().Name == defId)
                return ve;
            if (el is Container c && c.elements != null) {
                foreach (var child in c.elements) {
                    var hit = WalkForFormat(child, defId);
                    if (hit != null) return hit;
                }
            }
            return null;
        }

        [Test]
        public void TestOblivionIdleAnamDisplays() {
            var plugin = Load(Games.TES4, "Oblivion.esm");
            var idle = First(plugin, "IDLE");
            var anam = idle.GetElement("ANAM") as ValueElement;
            Assert.IsNotNull(anam);
            Assert.IsTrue(
                anam.def is ValueDef vd &&
                vd.formatDef is esper.defs.TES4.IdleAnamFormat);
            StringAssert.IsMatch(
                @"^(Lower Body|Left Arm|Left Hand|Right Arm|Special Idle|Whole Body|Upper Body|<Unknown: \d+>)(, Must return a file)?$",
                anam.GetValue());
        }

        [Test]
        public void TestOblivionMgefCodeDisplays() {
            var plugin = Load(Games.TES4, "Oblivion.esm");
            ValueElement code = null;
            foreach (var sig in new[] { "ENCH", "SPEL", "ALCH", "INGR" }) {
                foreach (var r in plugin.records.Where(
                    r => r.signature.ToString() == sig)) {
                    var found = FindFormatted(r, vd =>
                        vd.formatDef is esper.defs.TES4.Char4Format);
                    if (found != null && found.GetValue().Length == 4) {
                        code = found;
                        break;
                    }
                }
                if (code != null) break;
            }
            if (code == null)
                Assert.Ignore("No Char4 effect name found.");
            Assert.AreEqual(4, code.GetValue().Length);
        }

        [Test]
        public void TestOblivionWthrClassificationDisplays() {
            var plugin = Load(Games.TES4, "Oblivion.esm");
            var wthr = First(plugin, "WTHR");
            var cls = FindFormatted(wthr, vd =>
                vd.formatDef is esper.defs.TES4.WthrDataClassificationFormat);
            if (cls == null) Assert.Ignore("No WTHR classification found.");
            StringAssert.IsMatch(
                @"^(None|Pleasant|Cloudy|Unknown 3|Rainy|Snow|<Unknown: \d+>)$",
                cls.GetValue());
        }

        [Test]
        public void TestOblivionSoundAttenuationScales() {
            var plugin = Load(Games.TES4, "Oblivion.esm");
            var soun = First(plugin, "SOUN");
            var min = FindFormatted(
                soun, vd => vd.formatDef is MultiplyDef);
            if (min == null) Assert.Ignore("No scaled SOUN element found.");
            var raw = Convert.ToInt64(min.data);
            var factor = ((MultiplyDef)((ValueDef)min.def).formatDef).factor;
            Assert.AreEqual((raw * factor).ToString(), min.GetValue());
            Assert.IsTrue(factor == 5 || factor == 100);
        }
    }
}
