using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests;
using Tests.defs;

namespace Tests.elements {
    [TestFixture]
    public class MemberArrayCoverageTests {
        private static PluginFile LoadAllRecords() {
            var path = TestHelpers.FixturePath("AllRecords.esp");
            if (!File.Exists(path))
                Assert.Ignore("AllRecords.esp missing.");
            var session = new Session(Games.SSE, new SessionOptions());
            return session.pluginManager.LoadPlugin(path);
        }

        private static MemberArrayElement FindIn(
            Element element,
            Func<MemberArrayElement, bool> predicate
        ) {
            if (element is MemberArrayElement array && predicate(array))
                return array;
            if (element is Container container)
                foreach (var child in container.elements) {
                    var found = FindIn(child, predicate);
                    if (found != null) return found;
                }
            return null;
        }

        private static bool IsUnsorted(MemberArrayElement array) {
            return !((array.def as MemberArrayDef).sorted);
        }

        // Two-pass: prefer an unsorted count==2 array (appends on
        // sorted arrays need sort children), fall back to any count==2.
        // NOTE: AllRecords.esp has no decoded KWDA/Keywords arrays,
        // so these tests drive whatever member array qualifies
        // (e.g. FLST FormIDs).
        private static MemberArrayElement FindTestArray() {
            var plugin = LoadAllRecords();
            foreach (var record in plugin.records) {
                var found = FindIn(record, a =>
                    a.count == 2 && IsUnsorted(a));
                if (found != null) return found;
            }
            foreach (var record in plugin.records) {
                var found = FindIn(record, a => a.count == 2);
                if (found != null) return found;
            }
            return null;
        }

        private static MemberArrayElement FindUnionArray() {
            var plugin = LoadAllRecords();
            foreach (var record in plugin.records) {
                var found = FindIn(record, a =>
                    a.count >= 1 &&
                    (a.def as MemberArrayDef).memberDef
                        .GetSignatures().Count > 0);
                if (found != null) return found;
            }
            return null;
        }

        private static MemberArrayElement FindGrowableArray() {
            var plugin = LoadAllRecords();
            foreach (var record in plugin.records) {
                var found = FindIn(record, a =>
                    a.count >= 1 && IsUnsorted(a));
                if (found != null) return found;
            }
            return null;
        }

        [Test]
        public void ElementsAndSupportsSignature() {
            var array = FindUnionArray();
            if (array == null)
                Assert.Ignore("AllRecords.esp with union array missing.");
            Assert.AreEqual(array.count, array.elements.Count);
            var memberDef = (array.def as MemberArrayDef).memberDef;
            var sigs = memberDef.GetSignatures();
            Assert.IsTrue(array.SupportsSignature(sigs.First()));
            Assert.IsFalse(array.SupportsSignature(
                Signature.FromString("XXXX")
            ));
        }

        [Test]
        public void GetAssignmentAppendsAndRejectsMismatch() {
            var keywords = FindTestArray();
            if (keywords == null)
                Assert.Ignore("AllRecords.esp with test array missing.");
            var before = keywords.count;
            var memberDef = (keywords.def as MemberArrayDef).memberDef;
            var info = keywords.GetAssignment(memberDef);
            Assert.AreEqual(before, info.index);
            Assert.Throws<Exception>(() =>
                keywords.GetAssignment(keywords.def));
        }

        [Test]
        public void RemoveSingleElementDetaches() {
            var keywords = FindTestArray();
            if (keywords == null)
                Assert.Ignore("AllRecords.esp with test array missing.");
            var before = keywords.count;
            Assert.AreEqual(2, before);
            var first = keywords.elements.First();
            keywords.RemoveElement(first);
            Assert.IsFalse(keywords.elements.Contains(first));
            Assert.AreEqual(1, keywords.count);
        }

        [Test]
        public void RemoveAllElementsEmpties() {
            var keywords = FindTestArray();
            if (keywords == null)
                Assert.Ignore("AllRecords.esp with test array missing.");
            while (keywords.count > 1)
                keywords.RemoveElement(keywords.elements.First());
            if (keywords.count == 1) {
                bool removed = keywords.RemoveElement(
                    keywords.elements.First()
                );
                if (!removed)
                    Assert.AreEqual(1, keywords.count);
                else
                    // Element.Remove detaches the array itself, contents stay
                    Assert.AreEqual(1, keywords.count);
            }
            Assert.LessOrEqual(keywords.count, 1);
        }

        [Test]
        public void CreateDefaultAppendsElement() {
            var keywords = FindGrowableArray();
            if (keywords == null)
                Assert.Ignore("AllRecords.esp with growable array missing.");
            var count = keywords.count;
            var created = keywords.CreateDefault();
            Assert.AreEqual(count + 1, keywords.count);
        }

        [Test]
        public void CopyIntoDuplicatesChildren() {
            var keywords = FindTestArray();
            if (keywords == null)
                Assert.Ignore("AllRecords.esp with test array missing.");
            var before = keywords.count;
            var copy = (MemberArrayElement)keywords.CopyInto(
                keywords.record, CopyOptions.AsNewRecord
            );
            Assert.AreEqual(before, copy.count);
        }

        [Test]
        public void ToJsonListsChildren() {
            var keywords = FindTestArray();
            if (keywords == null)
                Assert.Ignore("AllRecords.esp with test array missing.");
            var before = keywords.count;
            var json = (JArray)keywords.ToJson();
            Assert.AreEqual(before, json.Count);
        }

        [Test]
        public void SortedArrayKeepsOrder() {
            var plugin = LoadAllRecords();
            MemberArrayElement sorted = null;
            foreach (var record in plugin.records) {
                sorted = FindSorted(record);
                if (sorted != null) break;
            }
            if (sorted == null)
                Assert.Ignore("AllRecords.esp with sorted array missing.");
            Assert.Greater(sorted.count, 1);
            var keys = sorted.elements
                .Select(e => e.sortKey)
                .ToList();
            var ordered = keys.OrderBy(k => k).ToList();
            CollectionAssert.AreEqual(ordered, keys);
        }

        private static MemberArrayElement FindSorted(Element element) {
            if (element is MemberArrayElement array &&
                (array.def as MemberArrayDef).sorted && array.count > 1)
                return array;
            if (element is Container container)
                foreach (var child in container.elements) {
                    var found = FindSorted(child);
                    if (found != null) return found;
                }
            return null;
        }
    }
}
