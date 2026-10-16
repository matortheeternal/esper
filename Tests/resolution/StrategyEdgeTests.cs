using esper;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.resolution.strategies;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.resolution {
    // Direct unit tests for resolution strategies that are not wired into
    // the public path API (ResolutionExtensions.strategies only wires 8 of
    // them), plus the tiny MatchData/AssignmentInfo/PluginRecordMap/
    // UnknownGroupDef types that had zero coverage. Uses the committed
    // AllRecords.esp fixture, so every case runs without local masters.
    [TestFixture]
    public class StrategyEdgeTests {
        private Session session;
        private PluginFile plugin;
        private GroupRecord group;
        private MainRecord rec;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("AllRecords.esp")
            );
            group = (GroupRecord)plugin.elements.First(e => {
                return e is GroupRecord g && g.isTopGroup
                    && g.groupDef.signature.ToString() == "ACTI";
            });
            rec = (MainRecord)group.elements.First(e => {
                return e is MainRecord;
            });
        }

        [Test]
        public void BaseStrategyDefaults() {
            var s = new ResolutionStrategy();
            Assert.IsTrue(s.canResolve);
            Assert.Throws<NotImplementedException>(() => {
                s.Match(rec, "anything");
            });
            Assert.Throws<NotImplementedException>(() => {
                s.Resolve(new MatchData(null));
            });
            Assert.Throws<NotImplementedException>(() => {
                s.Create(new MatchData(null));
            });
        }

        [Test]
        public void MatchDataWrapsMatch() {
            var m = new Regex("a+").Match("aaa");
            Assert.AreSame(m, new MatchData(m).match);
        }

        [Test]
        public void ElementMatchRequiresTargetType() {
            var expr = new Regex(".*");
            var value = rec.GetElement("EDID");
            Assert.IsNotNull(value);
            Assert.IsNull(ElementMatch<Container>.From(value, ".", expr));
            var match = ElementMatch<Container>.From(rec, ".", expr);
            Assert.IsNotNull(match);
            Assert.AreSame(rec, match.target);
        }

        [Test]
        public void FormIdMatchRequiresContainer() {
            var expr = new Regex("^[0-9A-F]{8}$");
            var value = rec.GetElement("EDID");
            Assert.IsNull(FormIdMatch.From(value, "01001234", expr));
            Assert.IsNull(FormIdMatch.From(group, "nope", expr));
            var match = FormIdMatch.From(group, "01001234", expr);
            Assert.IsNotNull(match);
            Assert.AreSame(group, match.target);
        }

        [Test]
        public void GlobalFormIdMatchSupportedElements() {
            var s = new ResolveByGlobalFormId();
            Assert.IsNotNull(s.Match(plugin, "01001234"));
            Assert.IsNotNull(s.Match(group, "01001234"));
            var root = plugin.container as RootElement;
            Assert.IsNotNull(root);
            Assert.IsNotNull(s.Match(root, "01001234"));
            Assert.IsNull(s.Match(rec, "01001234"));
            Assert.IsNull(s.Match(plugin, "XYZ"));
            Assert.IsNull(s.Match(plugin, "123"));
        }

        [Test]
        public void FullNameMatchRequiresQuotedTopGroup() {
            var s = new ResolveByFullName();
            Assert.IsNotNull(s.Match(group, "\"Something\""));
            Assert.IsNull(s.Match(group, "Something"));
            Assert.IsNull(s.Match(rec, "\"Something\""));
        }

        [Test]
        public void FullNameResolveFindsRecordByFullName() {
            var target = plugin.records.FirstOrDefault(r => {
                return !string.IsNullOrEmpty(r.fullName);
            });
            Assert.IsNotNull(target, "AllRecords.esp has no FULL-bearing record");
            var s = new ResolveByFullName();
            var top = (GroupRecord)target.container;
            var found = s.Resolve(s.Match(top, $"\"{target.fullName}\""));
            Assert.AreSame(target, found);
            Assert.IsNull(s.Resolve(s.Match(top, "\"No Such Name\"")));
        }

        [Test]
        public void CreateByDefaultMatchesDotAndPlus() {
            var s = new CreateByDefault();
            Assert.IsFalse(s.canResolve);
            Assert.IsNotNull(s.Match(rec, "."));
            Assert.IsNotNull(s.Match(rec, "+"));
            Assert.IsNotNull(s.Match(rec, "[+]"));
            Assert.IsNull(s.Match(rec, "x"));
            var value = rec.GetElement("EDID");
            Assert.IsNotNull(value);
            Assert.IsNull(s.Match(value, "."));
        }

        [Test]
        public void CreateByDefaultCreatesArrayItem() {
            var array = plugin.records
                .SelectMany(r => r.GetElements())
                .OfType<ArrayElement>()
                .FirstOrDefault(a => !a.arrayDef.sorted);
            Assert.IsNotNull(array, "AllRecords.esp has no unsorted array");
            var s = new CreateByDefault();
            var before = array.elements.Count;
            var created = s.Create(s.Match(array, "."));
            Assert.IsNotNull(created);
            Assert.AreEqual(before + 1, array.elements.Count);
        }

        [Test]
        public void ChildGroupResolveAndCreate() {
            var s = new ResolveChildGroup();
            Assert.IsNotNull(s.Match(rec, "Child Group"));
            Assert.IsNull(s.Match(rec, "Other"));
            var obnd = rec.GetElement("OBND") as Container;
            Assert.IsNotNull(obnd);
            Assert.IsNull(s.Match(obnd, "Child Group"));
            // ACTI records carry no child group.
            Assert.IsNull(s.Resolve(s.Match(rec, "Child Group")));
            // A record that already parents a child group resolves it.
            var withChild = plugin.records.FirstOrDefault(r => {
                return r.childGroup != null;
            });
            Assert.IsNotNull(
                withChild, "AllRecords.esp has no record with a child group"
            );
            Assert.AreSame(
                withChild.childGroup,
                s.Resolve(s.Match(withChild, "Child Group"))
            );
            // Detaching the existing child group lets Create build a fresh
            // one, covering both CreateChildGroup branches.
            if (withChild.GetChildGroupDef() == null)
                Assert.Ignore("Record parent group def has no child group");
            withChild.container.internalElements.Remove(withChild.childGroup);
            withChild.childGroup = null;
            Assert.IsNull(s.Resolve(s.Match(withChild, "Child Group")));
            var created = s.Create(s.Match(withChild, "Child Group"));
            Assert.IsNotNull(created);
            Assert.AreSame(
                created, s.Resolve(s.Match(withChild, "Child Group"))
            );
            Assert.AreSame(created, s.Create(s.Match(withChild, "Child Group")));
        }

        [Test]
        public void AssignmentInfoInsertAndReplace() {
            var array = plugin.records
                .SelectMany(r => r.GetElements())
                .OfType<ArrayElement>()
                .FirstOrDefault();
            Assert.IsNotNull(array, "AllRecords.esp has no array element");
            var first = array.internalElements[0];
            var before = array.internalElements.Count;
            new AssignmentInfo().Assign(array, first, false);
            Assert.AreEqual(before + 1, array.internalElements.Count);
            Assert.AreSame(first, array.internalElements[0]);
            var replace = new AssignmentInfo {
                index = 0, assigned = true
            };
            replace.Assign(array, first, true);
            Assert.AreEqual(before + 1, array.internalElements.Count);
            Assert.AreSame(first, array.internalElements[0]);
            var fresh = new AssignmentInfo();
            Assert.AreEqual(0, fresh.index);
            Assert.IsFalse(fresh.assigned);
        }

        [Test]
        public void PluginRecordMapAutoCreatesMaps() {
            var maps = new PluginRecordMap();
            maps.Add(plugin, rec);
            Assert.AreSame(rec, maps.GetMap(plugin).Get(rec.localFormId));
            var other = new PluginRecordMap();
            other.AddMap(plugin);
            Assert.IsNotNull(other.GetMap(plugin));
        }

        [Test]
        public void UnknownGroupDefMetadataAndLabels() {
            var def = new UnknownGroupDef(
                session.definitionManager, new JObject()
            );
            Assert.AreEqual(10, def.groupType);
            Assert.IsTrue(def.hasRecordParent);
            Assert.IsTrue(def.isChildGroup);
            Assert.AreEqual("Unknown", def.name);
            Assert.AreEqual(((esper.data.Signature)group.label).bytes, def.ParseLabel(group, "x"));
            Assert.Throws<Exception>(() => def.ParseLabel(rec, "x"));
            var formId = (esper.data.FormId)def.ConvertLabel(group, ((esper.data.Signature)group.label).bytes);
            Assert.AreEqual(
                BitConverter.ToUInt32(((esper.data.Signature)group.label).bytes), formId.fileFormId
            );
        }
    }
}
