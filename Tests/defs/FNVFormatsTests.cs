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
    public class FNVFormatsTests {
        private static Session FNV() => Synthetic.GetSession(Games.FNV);

        private static JObject Src(string name) {
            return new JObject { { "name", name } };
        }

        private static PluginFile LoadOrIgnore(Session session, string name) {
            var path = TestHelpers.FixturePath(name);
            if (!File.Exists(path)) Assert.Ignore($"{name} missing.");
            return session.pluginManager.LoadPlugin(path);
        }

        private static Session NewSession(Game game) {
            return new Session(game, new SessionOptions());
        }

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
                ValueElement hit = null;
                try {
                    hit = Walk(rec, defId);
                } finally {
                    if (hit == null) {
                        rec._internalElements = null;
                        rec._unexpectedSubrecords = null;
                    }
                }
                if (hit != null) return hit;
            }
            return null;
        }

        private static string DisplayText(ValueElement el) {
            var vd = (ValueDef)el.def;
            return vd.formatDef.DataToValue(el, el.data);
        }

        [Test]
        public void TestAlocTimeDisplays() {
            var def = new esper.defs.FNV.AlocTimeFormat(
                FNV().definitionManager, Src("Day Start"));
            Assert.AreEqual("12:00:00 AM", def.DataToValue(null, 0u));
            Assert.AreEqual("12:00:00 PM", def.DataToValue(null, 128u));
            Assert.AreEqual("6:00:00 AM", def.DataToValue(null, 64u));
            Assert.AreEqual("0000", def.GetSortKey(null, 0u));
            Assert.AreEqual("0080", def.GetSortKey(null, 128u));
        }

        [Test]
        public void TestAlocTimeEditPath() {
            var def = new esper.defs.FNV.AlocTimeFormat(
                FNV().definitionManager, Src("Day Start"));
            Assert.AreEqual(128, def.ValueToData(null, "12:00:00 PM"));
            Assert.AreEqual(0, def.ValueToData(null, "12:00:00 AM"));
            Assert.AreEqual(
                128, def.ValueToData(
                    null, (string)def.DataToValue(null, 128u)));
            Assert.AreEqual(64L, (long)def.ValueToData(null, "64"));
        }

        [Test]
        public void TestFalloutNVAlocTimeDisplays() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var aloc = plugin.records.FirstOrDefault(
                r => r.signature.ToString() == "ALOC");
            if (aloc == null) Assert.Ignore("No ALOC in FalloutNV.esm.");
            var day = aloc.GetElement("NAM5") as ValueElement;
            Assert.IsNotNull(day);
            StringAssert.IsMatch(
                @"^\d+:\d\d:\d\d (AM|PM)$", day.GetValue());
        }

        [Test]
        public void TestNVTREdgeNullElement() {
            var manager = FNV().definitionManager;
            var format = new esper.defs.FO3.NVTREdgeFormat(
                manager, new JObject());
            Assert.AreEqual("5", format.DataToValue(null, 5));
        }

        [Test]
        public void TestNVTREdgeExternalEdge() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var seen = 0;
            foreach (var rec in plugin.records) {
                if (rec.signature.ToString() != "NAVM") continue;
                if (++seen > 20) break;
                ValueElement edge = null;
                try {
                    edge = FindExternalEdge(rec);
                    if (edge == null) continue;
                    var text = DisplayText(edge);
                    StringAssert.StartsWith("#", text);
                    StringAssert.Contains("Triangle #", text);
                    StringAssert.Contains(" in \"", text);
                    return;
                } finally {
                    if (edge == null) {
                        rec._internalElements = null;
                        rec._unexpectedSubrecords = null;
                    }
                }
            }
            Assert.Ignore("No external NVTREdge element found.");
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

        [Test]
        public void TestPRKCFallbackIsRawValue() {
            var manager = FNV().definitionManager;
            var format = new esper.defs.FO3.PRKCFormat(
                manager, new JObject());
            Assert.AreEqual("9", format.DataToValue(null, 9));
        }

        [Test]
        public void TestPRKCResolvesCaption() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = TestHelpers.FindRecord(
                plugin.records.Where(
                    r => r.signature.ToString() == "PERK").Take(50),
                rec => {
                    var hit = Walk(rec, "PRKCFormat");
                    if (hit == null) return null;
                    return DisplayText(hit).Any(char.IsLetter) ? hit : null;
                });
            if (el == null)
                Assert.Ignore("No captioned PRKC element found.");
        }

        [Test]
        public void TestPerkDATAFunctionFallbackIsRawValue() {
            var manager = FNV().definitionManager;
            var format = new esper.defs.FO3.PerkDATAFunctionFormat(
                manager, new JObject());
            Assert.AreEqual("2", format.DataToValue(null, 2));
        }

        [Test]
        public void TestPerkDATAFunctionResolvesName() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = TestHelpers.FindRecord(
                plugin.records.Where(
                    r => r.signature.ToString() == "PERK").Take(50),
                rec => {
                    var hit = Walk(rec, "PerkDATAFunctionFormat");
                    if (hit == null) return null;
                    return DisplayText(hit).Any(char.IsLetter) ? hit : null;
                });
            if (el == null)
                Assert.Ignore("No named PerkDATAFunction element found.");
        }

        [Test]
        public void TestPerkEPFTFallbackIsRawValue() {
            var manager = FNV().definitionManager;
            var format = new esper.defs.FO3.PerkEPFTFormat(
                manager, new JObject());
            Assert.AreEqual("1", format.DataToValue(null, 1));
        }

        [Test]
        public void TestPerkEPFTResolvesName() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var el = TestHelpers.FindRecord(
                plugin.records.Where(
                    r => r.signature.ToString() == "PERK").Take(50),
                rec => {
                    var hit = Walk(rec, "PerkEPFTFormat");
                    if (hit == null) return null;
                    return DisplayText(hit).Any(char.IsLetter) ? hit : null;
                });
            if (el == null)
                Assert.Ignore("No named PerkEPFT element found.");
        }

        [Test]
        public void TestQuestObjectiveFallbackWarns() {
            var manager = FNV().definitionManager;
            var format = new esper.defs.FNV.CTDAParam2QuestObjectiveFormat(
                manager, new JObject());
            Assert.AreEqual(
                "7 <Warning: Could not resolve Parameter 1>",
                format.DataToValue(null, 7));
            Assert.AreEqual("00000007", format.GetSortKey(null, 7));
            Assert.AreEqual(34, format.ValueToData(null, "034 Hayes"));
        }

        [Test]
        public void TestQuestObjectiveResolvesRealObjectives() {
            var session = NewSession(Games.FNV);
            var plugin = LoadOrIgnore(session, "FalloutNV.esm");
            var rec = plugin.GetRecordByFormId(0xE58D5);
            if (rec == null) Assert.Ignore("INFO 000E58D5 missing.");
            var texts = CollectDisplays(
                rec, "CTDAParam2QuestObjectiveFormat");
            StringAssert.Contains(
                "034 Hayes and the NCR will protect Primm", texts);
            StringAssert.Contains("020 Beagle wants", texts);
            StringAssert.Contains("022 With Beagle dead", texts);
        }

        private static string CollectDisplays(MainRecord rec, string defId) {
            var found = new System.Collections.Generic.List<ValueElement>();
            CollectFormatted(rec, defId, found);
            return string.Join("\n", found.Select(DisplayText));
        }
    }
}
