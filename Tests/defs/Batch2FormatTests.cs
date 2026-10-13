using esper;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Tests;

namespace Tests.defs {
    public class Batch2FormatTests {
        private static ValueElement Walk(Element el, string defId) {
            if (el is ValueElement ve && ve.def is ValueDef vd &&
                vd.formatDef != null && vd.formatDef.GetType().Name == defId)
                return ve;
            if (el is Container c && c.elements != null) {
                foreach (var child in c.elements) {
                    var hit = Walk(child, defId);
                    if (hit != null) return hit;
                }
            }
            return null;
        }

        private static ValueElement FindFormatted(
            PluginFile plugin, string sig, string defId, int maxRecords
        ) {
            var seen = 0;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != sig) continue;
                if (++seen > maxRecords) break;
                var hit = Walk(rec, defId);
                if (hit != null) return hit;
            }
            return null;
        }

        private static ValueElement FindExternalEdge(MainRecord rec) {
            var found = new System.Collections.Generic.List<ValueElement>();
            CollectFormatted(rec, "NVTREdgeFormat", found);
            foreach (var edge in found) {
                if (DisplayText(edge).Contains("Triangle #")) return edge;
            }
            return null;
        }

        private static void CollectFormatted(
            Element el, string defId,
            System.Collections.Generic.List<ValueElement> found
        ) {
            if (el is ValueElement ve && ve.def is ValueDef vd &&
                vd.formatDef != null && vd.formatDef.GetType().Name == defId)
                found.Add(ve);
            if (el is Container c && c.elements != null) {
                foreach (var child in c.elements)
                    CollectFormatted(child, defId, found);
            }
        }

        private static string DisplayText(ValueElement el) {
            var vd = (ValueDef)el.def;
            return vd.formatDef.DataToValue(el, el.data);
        }

        private static PluginFile LoadOrIgnore(Session session, string name) {
            var path = TestHelpers.FixturePath(name);
            if (!File.Exists(path)) Assert.Ignore($"{name} missing.");
            return session.pluginManager.LoadPlugin(path);
        }

        private static Session NewSession(Game game) {
            return new Session(game, new SessionOptions());
        }

        [Test]
        public void VariableNameFallbackIsRawValue() {
            var manager = Synthetic.GetSession(Games.TES4).definitionManager;
            var format = new esper.defs.TES4.CTDAParam2VariableNameFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual("5", format.DataToValue(null, 5));
        }

        [Test]
        public void VariableNameValueToDataParsesNumber() {
            var manager = Synthetic.GetSession(Games.TES4).definitionManager;
            var format = new esper.defs.TES4.CTDAParam2VariableNameFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual(12L, format.ValueToData(null, "12"));
        }

        [Test]
        public void VariableNameWiringOblivion() {
            var session = NewSession(Games.TES4);
            var plugin = LoadOrIgnore(session, "Oblivion.esm");
            var el = FindFormatted(
                plugin, "INFO", "CTDAParam2VariableNameFormat", 500);
            if (el == null) Assert.Ignore("No VariableName element found.");
            var text = DisplayText(el);
            Assert.IsNotNull(text);
        }

        [Test]
        public void QuestObjectiveFallbackIsRawValue() {
            var manager = Synthetic.GetSession(Games.FNV).definitionManager;
            var format = new esper.defs.FNV.CTDAParam2QuestObjectiveFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual("7", format.DataToValue(null, 7));
        }

        [Test]
        public void QuestObjectiveWiringFNV() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = FindFormatted(
                plugin, "INFO", "CTDAParam2QuestObjectiveFormat", 1000);
            if (el == null) Assert.Ignore("No QuestObjective element found.");
            Assert.IsNotEmpty(DisplayText(el));
        }

        [Test]
        public void NVTREdgeNullElement() {
            var manager = Synthetic.GetSession(Games.FNV).definitionManager;
            var format = new esper.defs.FO3.NVTREdgeFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual("5", format.DataToValue(null, 5));
        }

        [Test]
        public void NVTREdgeWiringFNV() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var seen = 0;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "NAVM") continue;
                if (++seen > 20) break;
                var edge = FindExternalEdge(rec);
                if (edge == null) continue;
                var text = DisplayText(edge);
                StringAssert.StartsWith("#", text);
                StringAssert.Contains("Triangle #", text);
                StringAssert.Contains(" in \"", text);
                return;
            }
            Assert.Ignore("No external NVTREdge element found.");
        }

        [Test]
        public void PRKCFallbackIsRawValue() {
            var manager = Synthetic.GetSession(Games.FNV).definitionManager;
            var format = new esper.defs.FO3.PRKCFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual("9", format.DataToValue(null, 9));
        }

        [Test]
        public void PRKCWiringFNV() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = FindFormatted(plugin, "PERK", "PRKCFormat", 50);
            if (el == null) Assert.Ignore("No PRKC element found.");
            Assert.IsNotEmpty(DisplayText(el));
        }

        [Test]
        public void PerkDATAFunctionFallbackIsRawValue() {
            var manager = Synthetic.GetSession(Games.FNV).definitionManager;
            var format = new esper.defs.FO3.PerkDATAFunctionFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual("2", format.DataToValue(null, 2));
        }

        [Test]
        public void PerkDATAFunctionWiringFNV() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = FindFormatted(
                plugin, "PERK", "PerkDATAFunctionFormat", 50);
            if (el == null) Assert.Ignore("No Function element found.");
            Assert.IsNotEmpty(DisplayText(el));
        }

        [Test]
        public void PerkEPFTFallbackIsRawValue() {
            var manager = Synthetic.GetSession(Games.FNV).definitionManager;
            var format = new esper.defs.FO3.PerkEPFTFormat(
                manager, new Newtonsoft.Json.Linq.JObject());
            Assert.AreEqual("1", format.DataToValue(null, 1));
        }

        [Test]
        public void PerkEPFTWiringFNV() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = FindFormatted(plugin, "PERK", "PerkEPFTFormat", 50);
            if (el == null) Assert.Ignore("No EPFT element found.");
            Assert.IsNotEmpty(DisplayText(el));
        }
    }
}
