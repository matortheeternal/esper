using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.resolution;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests.defs {
    [TestFixture]
    public class UnionSortKeyTests {
        private static PluginFile LoadSharedSkyrim() {
            return Synthetic.GetPlugin(Games.SSE, "Skyrim.esm");
        }

        private static void Walk(Element el, List<Element> found) {
            found.Add(el);
            if (el is Container c && c.elements != null)
                foreach (var child in c.elements)
                    Walk(child, found);
        }

        private static ElementDef WalkDefs(ElementDef def, System.Func<ElementDef, bool> match) {
            if (match(def)) return def;
            if (def.childDefs == null) return null;
            foreach (var child in def.childDefs) {
                var hit = WalkDefs(child, match);
                if (hit != null) return hit;
            }
            return null;
        }

        [Test]
        public void SortedConditionsEnumerateWithoutThrow() {
            var plugin = LoadSharedSkyrim();
            if (plugin == null) Assert.Ignore("Skyrim.esm missing.");
            var info = plugin.GetRecordByFormId(0xDD7);
            Assert.IsNotNull(info, "INFO DD7 missing.");
            var conditions = info.GetElements("Conditions");
            Assert.IsNotNull(conditions, "No Conditions member on DD7.");
            Assert.Greater(conditions.Count, 0, "No conditions on DD7.");
            var structs = 0;
            foreach (var entry in conditions) {
                var all = new List<Element>();
                Walk(entry, all);
                foreach (var el in all) {
                    if (el is MemberArrayElement)
                        Assert.DoesNotThrow(() => { var _ = el.elements; });
                    if (el is StructElement) {
                        structs++;
                        Assert.DoesNotThrow(() => { var _ = el.sortKey; });
                    }
                    if (el is UnionElement)
                        Assert.DoesNotThrow(() => { var _ = el.sortKey; });
                }
            }
            Assert.Greater(structs, 0, "No structs under DD7 Conditions.");
        }

        [Test]
        public void UnionSortKeyDelegatesToFirstChild() {
            var plugin = LoadSharedSkyrim();
            if (plugin == null) Assert.Ignore("Skyrim.esm missing.");
            var perk = plugin.GetRecordByFormId(0x5820A);
            Assert.IsNotNull(perk, "PERK 5820A missing.");
            var all = new List<Element>();
            Walk(perk, all);
            UnionElement union = null;
            foreach (var el in all)
                if (el is UnionElement u) {
                    union = u;
                    break;
                }
            Assert.IsNotNull(union, "No UnionElement under PERK 5820A.");
            var children = ((Container)union).elements;
            Assert.Greater(children.Count, 0, "UnionElement has no children.");
            Assert.AreEqual(children[0].sortKey, union.sortKey);
        }

        [Test]
        public void EmptyUnionSortKeyIsEmpty() {
            var manager = Synthetic.GetSession(Games.SSE).definitionManager;
            var unionDef = WalkDefs(
                manager.GetRecordDef(Signature.FromString("INFO")), d => d is UnionDef
            );
            Assert.IsNotNull(unionDef, "No UnionDef under INFO.");
            var emptyUnion = new UnionElement(null, unionDef);
            Assert.AreEqual("", unionDef.GetSortKey(emptyUnion));
            var memberUnionDef = WalkDefs(
                manager.GetRecordDef(Signature.FromString("QUST")), d => d is MemberUnionDef
            );
            Assert.IsNotNull(memberUnionDef, "No MemberUnionDef under QUST.");
            var emptyMemberUnion = new MemberUnionElement(null, memberUnionDef);
            Assert.AreEqual("", memberUnionDef.GetSortKey(emptyMemberUnion));
        }
    }
}
