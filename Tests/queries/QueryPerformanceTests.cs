#nullable enable

using esper;
using esper.elements;
using esper.plugins;
using esper.queries;
using esper.setup;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Tests.queries {
    public class QueryPerformanceTests {
        private Session session = null!;
        private PluginFile skyrim = null!;

        [OneTimeSetUp]
        public void SetUp() {
            var path = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(path))
                Assert.Ignore("Local master fixture not found: Skyrim.esm");
            session = new Session(Games.TES5, new SessionOptions());
            skyrim = session.pluginManager.LoadPlugin(path);
        }

        [Test]
        public void TestSkyrimLoaded() {
            Assert.That(skyrim.records.Count, Is.GreaterThan(0));
        }

        [Test]
        [Timeout(5000)]
        public void TestArmoElements() {
            QueryResult result = Query.Parse(
                "SHOW elements WHERE record.signature = 'ARMO'"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements.Count, Is.GreaterThan(0));
        }

        [Test]
        [Timeout(5000)]
        public void TestRecordsNumericFilter() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE fileFormId > 0"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements.Count, Is.GreaterThan(0));
        }

        [Test]
        [Timeout(5000)]
        public void TestRecordsFilterAndOrder() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE signature = 'ARMO' ORDER BY editorId"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements.Count, Is.GreaterThan(0));
        }

        [Test]
        [Timeout(5000)]
        public void TestGroupBySignature() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY signature"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements.Count, Is.GreaterThan(0));
            Assert.That(result.elements, Is.All.TypeOf<MainRecord>());
            var signatures = result.elements.Cast<MainRecord>()
                .Select(record => record.signature.ToString()).ToHashSet();
            GroupNode[] groups = result.root.children.Cast<GroupNode>().ToArray();
            Assert.That(groups.Length, Is.EqualTo(signatures.Count));
            Assert.That(
                groups.Sum(group => group.elements.Count()),
                Is.EqualTo(result.elements.Count)
            );
        }

        [Test]
        [Timeout(15000)]
        public void TestRefrPlacingArmor() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE signature = 'REFR' AND [@NAME].signature = 'ARMO'"
            ).Run(session);
            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements.Count, Is.GreaterThan(0));
        }

        [Test]
        [Timeout(15000)]
        public void TestRefrNameSignatureComparison() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE signature = 'REFR' AND [@NAME].signature > 'ZZZZ'"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements, Is.Empty);
        }
    }
}
