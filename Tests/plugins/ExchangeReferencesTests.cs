using System.Collections.Generic;
using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    public class ExchangeReferencesTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        public PluginFile plugin;

        [SetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions {});
            var pluginPath = TestHelpers.FixturePath("AllRecords.esp");
            plugin = pluginManager.LoadPlugin(pluginPath);
            Assert.IsNotNull(plugin);
        }

        private static ValueElement FindFormRef(Element el) {
            if (el is ValueElement ve && ve.def is FormIdDef
                && ve.data is VariantData.Form form
                && form.V.fileFormId != 0)
                return ve;
            if (el is Container c)
                foreach (var child in c.elements) {
                    var found = FindFormRef(child);
                    if (found != null) return found;
                }
            return null;
        }

        private static MainRecord FirstRecord(PluginFile file) {
            foreach (var group in file.elements) {
                if (group is Container c)
                    foreach (var el in c.elements)
                        if (el is MainRecord rec) return rec;
            }
            return null;
        }

        [Test]
        public void TestExchangeReferencesRetargetsFormId() {
            var records = new List<MainRecord>();
            foreach (var group in plugin.elements) {
                if (group is not Container c) continue;
                foreach (var el in c.elements) {
                    if (el is MainRecord rec
                        && rec.GetElement("YNAM") is ValueElement)
                        records.Add(rec);
                }
            }
            Assert.GreaterOrEqual(records.Count, 3,
                "fixture should contain records with a YNAM field");
            var holder = records[0];
            var ynam = (ValueElement)holder.GetElement("YNAM");
            ynam.referencedRecord = records[1];
            var oldId = ((VariantData.Form)ynam.data).V;
            var newId = FormId.FromSource(plugin, records[2].localFormId);

            Assert.IsTrue(holder.ExchangeReferences(oldId, newId));
            var after = ((VariantData.Form)ynam.data).V;
            Assert.AreEqual(newId.fileFormId, after.fileFormId);
            Assert.IsTrue(holder.state.HasFlag(ElementState.Modified));
        }

        [Test]
        public void TestExchangeReferencesSkipsRecordIdentity() {
            var rec = FirstRecord(plugin);
            Assert.IsNotNull(rec);
            var ownId = FormId.FromSource(plugin, rec.localFormId);
            var target = FormId.FromSource(plugin, 0x000001u);
            Assert.IsFalse(rec.ExchangeReferences(ownId, target),
                "the header FormID is identity, not a reference");
        }

        [Test]
        public void TestExchangeReferencesAbsentIdReturnsFalse() {
            var rec = FirstRecord(plugin);
            Assert.IsNotNull(rec);
            var absent = FormId.FromSource(plugin, 0xFFFFFFu);
            var target = FormId.FromSource(plugin, 0x000001u);
            Assert.IsFalse(rec.ExchangeReferences(absent, target));
        }
    }
}
