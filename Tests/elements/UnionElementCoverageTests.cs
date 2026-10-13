using esper;
using esper.data;
using esper.defs;
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

namespace Tests.elements {
    [TestFixture]
    public class UnionElementCoverageTests {
        private static Session cachedSession;

        private static PluginFile LoadAllRecords() {
            var path = TestHelpers.FixturePath("AllRecords.esp");
            if (!File.Exists(path)) return null;
            if (cachedSession == null)
                cachedSession = new Session(Games.SSE, new SessionOptions());
            return cachedSession.pluginManager.LoadPlugin(path);
        }

        private static void Walk(Element element, List<Element> acc) {
            acc.Add(element);
            if (element is Container container)
                foreach (var child in container.elements)
                    Walk(child, acc);
        }

        private static UnionElement FindUnion(PluginFile plugin) {
            foreach (var rec in plugin.records) {
                var all = new List<Element>();
                Walk(rec, all);
                var hit = all.OfType<UnionElement>()
                    .FirstOrDefault(e => e.elements.Count > 0);
                if (hit != null) return hit;
            }
            return null;
        }

        private static MemberUnionElement FindMemberUnion(PluginFile plugin) {
            foreach (var rec in plugin.records) {
                var all = new List<Element>();
                Walk(rec, all);
                var hit = all.OfType<MemberUnionElement>()
                    .FirstOrDefault(e => e.elements.Count > 0);
                if (hit != null) return hit;
            }
            return null;
        }

        [Test]
        public void UnionDefProp() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindUnion(plugin);
            if (union == null) Assert.Ignore("No decoded union.");
            Assert.IsNotNull(union.unionDef);
            Assert.IsInstanceOf<UnionDef>(union.unionDef);
        }

        [Test]
        public void UnionGetAssignmentThrows() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindUnion(plugin);
            if (union == null) Assert.Ignore("No decoded union.");
            Assert.Throws<Exception>(
                () => union.GetAssignment(union.unionDef)
            );
        }

        [Test]
        public void UnionInitializeResolvesChild() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindUnion(plugin);
            if (union == null) Assert.Ignore("No decoded union.");
            var before = union.elements.Count;
            union.Initialize();
            Assert.Greater(union.elements.Count, before);
        }

        [Test]
        public void UnionRemoveElementIsRefusedByStructParent() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindUnion(plugin);
            if (union == null) Assert.Ignore("No decoded union.");
            var parent = union.container;
            Assert.IsNotNull(parent);
            // StructElement.RemoveElement unconditionally refuses, so a
            // union nested in a struct cannot be detached this way.
            Assert.IsFalse(union.RemoveElement(union.elements[0]));
            Assert.AreEqual(parent, union.container);
            Assert.IsTrue(parent.elements.Contains(union));
        }

        [Test]
        public void UnionCopyIntoDuplicatesChildren() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindUnion(plugin);
            if (union == null) Assert.Ignore("No decoded union.");
            var copy = union.CopyInto(
                union.container, CopyOptions.AsNewRecord
            ) as UnionElement;
            Assert.IsNotNull(copy);
            Assert.AreEqual(union.elements.Count, copy.elements.Count);
        }

        [Test]
        public void MemberUnionSupportsSignature() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindMemberUnion(plugin);
            if (union == null) Assert.Ignore("No decoded member union.");
            Assert.IsNotNull(union.unionDef);
            Assert.IsNotNull(union.unionDef.defaultDef);
            var childSig = union.unionDef.defaultDef.signature;
            Assert.IsTrue(union.SupportsSignature(childSig));
            Assert.IsFalse(
                union.SupportsSignature(Signature.FromString("XXXX"))
            );
        }

        [Test]
        public void MemberUnionGetAssignmentThrows() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindMemberUnion(plugin);
            if (union == null) Assert.Ignore("No decoded member union.");
            Assert.Throws<Exception>(
                () => union.GetAssignment(union.unionDef)
            );
        }

        [Test]
        public void MemberUnionRemoveElementDetaches() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindMemberUnion(plugin);
            if (union == null) Assert.Ignore("No decoded member union.");
            var parent = union.container;
            Assert.IsNotNull(parent);
            Assert.IsTrue(union.RemoveElement(union.elements[0]));
            Assert.IsFalse(parent.elements.Contains(union));
        }

        [Test]
        public void MemberUnionCopyIntoDuplicatesChildren() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindMemberUnion(plugin);
            if (union == null) Assert.Ignore("No decoded member union.");
            var copy = union.CopyInto(
                union.container, CopyOptions.AsNewRecord
            ) as MemberUnionElement;
            Assert.IsNotNull(copy);
            Assert.AreEqual(union.elements.Count, copy.elements.Count);
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
        public void MemberUnionInitializeAttachesDefaultBranch() {
            var plugin = LoadAllRecords();
            if (plugin == null) Assert.Ignore("AllRecords.esp missing.");
            var union = FindMemberUnion(plugin);
            if (union == null) Assert.Ignore("No decoded member union.");
            var parent = NewStruct(Games.SSE,
                @"{ 'name': 'S', 'type': 'struct', 'elements': [] }");
            var fresh = new MemberUnionElement(parent, union.unionDef);
            fresh.Initialize();
            Assert.IsNotNull(fresh.elements);
            Assert.Greater(fresh.elements.Count, 0);
            Assert.IsTrue(
                fresh.SupportsSignature(union.unionDef.defaultDef.signature)
            );
        }
    }
}
