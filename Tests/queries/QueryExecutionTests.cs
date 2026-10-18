#nullable enable

using esper;
using esper.elements;
using esper.plugins;
using esper.queries;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.queries {
    public class QueryExecutionTests {
        private Session session = null!;
        private PluginFile plugin = null!;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("ARecords.esp")
            );
        }

        [Test]
        public void TestShowRecordsReturnsLoadedSessionRecords() {
            QueryResult result = Query.Parse("SHOW records").Run(session);

            Assert.That(result.elements, Is.EqualTo(plugin.records));
            Assert.That(result.elements, Has.All.TypeOf<MainRecord>());
        }

        [Test]
        public void TestWhereAndOrderRunAgainstRecordProperties() {
            QueryResult result = Query.Parse(
                "SHOW records "
                + "WHERE signature IN (AACT, ACTI, ARMO) "
                + "ORDER BY local DESC ORDER BY signature DESC"
            ).Run(session);

            Assert.That(
                result.elements.Cast<MainRecord>().Select(record => record.editorId),
                Is.EqualTo(new[] { "TestArmor", "TestActivator", "TestAction" })
            );
        }

        [Test]
        public void TestNumericRecordPropertyComparison() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE fileFormId > 0"
            ).Run(session);

            Assert.That(result.elements, Is.EqualTo(plugin.records));
        }

        [TestCase(
            "signature = AACT OR signature = ARMO",
            2
        )]
        [TestCase("NOT signature = AACT", 8)]
        [TestCase("signature != AACT", 8)]
        [TestCase("signature MATCHES '^A(?:ACT|RMO)$'", 2)]
        [TestCase("editorId = 'testaction'", 1)]
        [TestCase("fileFormId >= 0", 9)]
        [TestCase("fileFormId <= 0", 0)]
        public void TestSupportedPredicatesRunAgainstRecords(
            string predicate,
            int expectedCount
        ) {
            QueryResult result = Query.Parse(
                $"SHOW records WHERE {predicate}"
            ).Run(session);

            Assert.That(result.elements, Has.Count.EqualTo(expectedCount));
        }

        [Test]
        public void TestShowElementsReturnsRecursiveRecordDescendants() {
            QueryResult result = Query.Parse("SHOW elements").Run(session);
            Element[] expected = plugin.records.SelectMany(descendants).ToArray();

            Assert.That(result.elements, Is.EqualTo(expected));
            Assert.That(result.elements, Has.None.TypeOf<MainRecord>());
            Assert.That(result.elements.Count, Is.GreaterThan(plugin.records.Count));
        }

        [Test]
        public void TestElementPropertiesFilterAfterTargetSelection() {
            QueryResult result = Query.Parse(
                "SHOW elements WHERE signature = 'EDID'"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Empty);
            Assert.That(
                result.elements.Select(element => element.signature.ToString()),
                Is.All.EqualTo("EDID")
            );
        }

        [Test]
        public void TestRecordScopedFiltersPruneElementCandidates() {
            QueryResult result = Query.Parse(
                "SHOW elements WHERE record.signature = 'ARMO'"
            ).Run(session);
            MainRecord armor = plugin.records.Single(
                record => record.editorId == "TestArmor"
            );

            Assert.That(result.elements, Is.EqualTo(descendants(armor)));
        }

        [Test]
        public void TestShowValuesFiltersRealRecordValues() {
            QueryResult result = Query.Parse(
                "SHOW values "
                + "WHERE signature = EDID AND value CONTAINS 'Armor' "
                + "ORDER BY value"
            ).Run(session);

            Assert.That(result.elements, Has.All.TypeOf<ValueElement>());
            Assert.That(
                result.elements.Cast<ValueElement>().Select(element => element.value),
                Is.EqualTo(new[] { "TestArmor", "TestArmorAddon" })
            );
        }

        [Test]
        public void TestDistinctValuesGroupByValueWithCounts() {
            QueryResult result = Query.Parse(
                "SHOW DISTINCT values WHERE signature = 'EDID' WITH COUNT"
            ).Run(session);

            GroupNode[] groups = result.root.children.Cast<GroupNode>().ToArray();
            Assert.That(groups.Select(group => group.key), Is.Unique);
            Assert.That(
                groups.Sum(group => (int)group.aggregateValues["COUNT"]!),
                Is.EqualTo(result.elements.Count)
            );
        }

        [Test]
        public void TestOrderByResolvesDirectValueElements() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE signature IN (ARMA, ARMO) "
                + "ORDER BY [DNAM] DESC"
            ).Run(session);

            Assert.That(
                result.elements.Cast<MainRecord>().Select(record => record.editorId),
                Is.EqualTo(new[] { "TestArmor", "TestArmorAddon" })
            );
        }

        [Test]
        public void TestCountReturnsImmediateChildren() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE editorId = 'TestArmorAddon' "
                + "AND [DNAM].count = 8"
            ).Run(session);

            Assert.That(result.elements, Has.Count.EqualTo(1));
        }

        [Test]
        public void TestResultTreeMirrorsReturnedElements() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE signature = AACT"
            ).Run(session);

            Assert.That(result.root.children, Has.Count.EqualTo(1));
            var node = (ElementNode)result.root.children[0];
            Assert.That(node.result, Is.SameAs(result));
            Assert.That(node.element, Is.SameAs(result.elements[0]));
        }

        [Test]
        public void TestGroupByBuildsGroupsWithoutDroppingElements() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY signature"
            ).Run(session);

            GroupNode[] groups = result.root.children.Cast<GroupNode>().ToArray();

            Assert.That(groups.Select(group => group.key), Is.All.TypeOf<string>());
            Assert.That(
                groups.SelectMany(group => group.elements),
                Is.EquivalentTo(result.elements)
            );
            foreach (GroupNode group in groups) {
                Assert.That(group.parent, Is.SameAs(result.root));
                Assert.That(group.elements, Is.Not.Empty);
            }
        }

        [Test]
        public void TestGroupByPreservesKeyTypes() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY fileFormId"
            ).Run(session);

            Assert.That(
                result.root.children.Cast<GroupNode>().Select(group => group.key),
                Is.All.TypeOf<uint>()
            );
        }

        [Test]
        public void TestMultipleGroupByClausesBuildNestedGroups() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY file GROUP BY signature"
            ).Run(session);

            GroupNode[] files = result.root.children.Cast<GroupNode>().ToArray();

            Assert.That(files.Length, Is.EqualTo(1));
            Assert.That(files[0].children, Is.All.TypeOf<GroupNode>());
            Assert.That(files[0].elements, Is.EquivalentTo(result.elements));
            foreach (TreeNode child in files[0].children)
                Assert.That(child.parent, Is.SameAs(files[0]));
        }

        [Test]
        public void TestOrderBySortsElementsWithinGroups() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY file ORDER BY editorId DESC"
            ).Run(session);

            GroupNode group = (GroupNode)result.root.children.Single();
            string[] editorIds = group.elements.Cast<MainRecord>()
                .Select(record => record.editorId)
                .ToArray();

            Assert.That(editorIds, Is.Ordered.Descending);
        }

        [Test]
        public void TestSourceAndTargetNodesExecuteAsPipeline() {
            Query query = Query.Parse("SHOW records FROM session");

            Element[] roots = query.source.Select(session).ToArray();
            Element[] selected = query.target.Select(roots).ToArray();

            Assert.That(roots, Is.EqualTo(plugin.records));
            Assert.That(selected, Is.EqualTo(plugin.records));
        }

        [Test]
        public void TestSelectionSourceUsesCapturedTreeElements() {
            QueryResult initial = Query.Parse(
                "SHOW records WHERE signature = ARMO"
            ).Run(session);
            QuerySelection selection = session.CaptureSelection(initial.root);

            QueryResult result = Query.Parse(
                $"SHOW records FROM selection {selection.id}"
            ).Run(session);

            Assert.That(result.elements, Is.EqualTo(initial.elements));
        }

        [Test]
        public void TestSelectionCapturePreservesInputOrderAndDuplicates() {
            QueryResult result = Query.Parse("SHOW records").Run(session);
            Element first = result.elements[0];
            Element second = result.elements[1];

            QuerySelection selection = session.CaptureSelection(second, first, second);

            Assert.That(selection.elements, Is.EqualTo(new[] { second, first, second }));
        }

        [Test]
        public void TestSelectionCaptureRejectsElementsFromAnotherSession() {
            var otherSession = new Session(Games.SSE, new SessionOptions());

            ArgumentException? error = Assert.Throws<ArgumentException>(
                () => session.CaptureSelection(otherSession.root)
            );

            Assert.That(error!.Message, Does.Contain("another session"));
        }

        [Test]
        public void TestSelectionSourceRejectsUnknownSelection() {
            InvalidOperationException? error = Assert.Throws<InvalidOperationException>(
                () => Query.Parse("SHOW records FROM selection 999999").Run(session)
            );

            Assert.That(error!.Message, Is.EqualTo("Selection '999999' does not exist."));
        }

        [Test]
        public void TestClauseNodesExecuteTheirExpressions() {
            Query query = Query.Parse(
                "SHOW records WHERE signature = ARMO ORDER BY editorId DESC"
            );
            MainRecord armor = plugin.records.Single(
                record => record.editorId == "TestArmor"
            );
            MainRecord action = plugin.records.Single(
                record => record.editorId == "TestAction"
            );

            Assert.That(query.whereClause!.Matches(armor), Is.True);
            Assert.That(query.whereClause.Matches(action), Is.False);
            Assert.That(query.orderBy[0].Compare(armor, action), Is.LessThan(0));
        }

        [Test]
        public void TestAggregateQueryReturnsAggregateValues() {
            QueryResult result = Query.Parse(
                "AGGREGATE records COUNT"
            ).Run(session);

            var node = (ValueNode)result.root.children.Single();
            Assert.That(result.elements, Is.Empty);
            Assert.That(node.value, Is.EqualTo(plugin.records.Count));
        }

        [Test]
        public void TestNumericAggregateQueries() {
            decimal[] formIds = plugin.records
                .Select(record => (decimal)record.fileFormId)
                .ToArray();

            Assert.That(AggregateValue("SUM"), Is.EqualTo(formIds.Sum()));
            Assert.That(AggregateValue("AVG"), Is.EqualTo(formIds.Average()));
            Assert.That(AggregateValue("MIN"), Is.EqualTo(formIds.Min()));
            Assert.That(AggregateValue("MAX"), Is.EqualTo(formIds.Max()));
        }

        [Test]
        public void TestGroupByWithCountAttachesAggregateValues() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY signature WITH COUNT"
            ).Run(session);

            foreach (GroupNode group in result.root.children.Cast<GroupNode>())
                Assert.That(
                    group.aggregateValues["COUNT"],
                    Is.EqualTo(group.elements.Count())
                );
        }

        [Test]
        public void TestGroupByWithSumAttachesAggregateValues() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY signature WITH SUM fileFormId"
            ).Run(session);

            foreach (GroupNode group in result.root.children.Cast<GroupNode>()) {
                decimal expected = group.elements.Cast<MainRecord>()
                    .Sum(record => (decimal)record.fileFormId);
                Assert.That(
                    group.aggregateValues["SUM fileFormId"],
                    Is.EqualTo(expected)
                );
            }
        }

        [Test]
        public void TestGroupByWithMultipleAggregatesAttachesValuesByText() {
            QueryResult result = Query.Parse(
                "SHOW records GROUP BY signature WITH COUNT "
                + "WITH SUM fileFormId WITH MAX dataSize"
            ).Run(session);

            foreach (GroupNode group in result.root.children.Cast<GroupNode>()) {
                MainRecord[] records = group.elements.Cast<MainRecord>().ToArray();
                Assert.That(group.aggregateValues["COUNT"], Is.EqualTo(records.Length));
                Assert.That(
                    group.aggregateValues["SUM fileFormId"],
                    Is.EqualTo(records.Sum(record => (decimal)record.fileFormId))
                );
                Assert.That(
                    group.aggregateValues["MAX dataSize"],
                    Is.EqualTo(records.Max(record => (decimal)record.dataSize))
                );
            }
        }

        [Test]
        public void TestUnquotedSignaturesRequireFourUppercaseCharacters() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE signature = ARMO"
            ).Run(session);

            Assert.That(result.elements, Has.Count.EqualTo(1));
            Assert.Throws<InvalidOperationException>(
                () => Query.Parse("SHOW records WHERE signature = armo").Run(session)
            );
        }

        [Test]
        public void TestNumericAggregateRejectsNonNumericValues() {
            InvalidOperationException? error = Assert.Throws<InvalidOperationException>(
                () => Query.Parse("AGGREGATE records SUM editorId").Run(session)
            );

            Assert.That(
                error!.Message,
                Is.EqualTo("SUM requires numeric values, not 'String'.")
            );
        }

        [TestCase(
            "SHOW records WHERE LOWER(editorId) = 'testaction'",
            "Query function 'LOWER' cannot be executed."
        )]
        public void TestUnsupportedExecutionIsExplicit(string text, string message) {
            NotSupportedException? error = Assert.Throws<NotSupportedException>(
                () => Query.Parse(text).Run(session)
            );
            Assert.That(error!.Message, Is.EqualTo(message));
        }

        [Test]
        public void TestInRequiresAnArrayAtExecution() {
            InvalidOperationException? error =
                Assert.Throws<InvalidOperationException>(
                    () => Query.Parse("SHOW records WHERE signature IN ARMO")
                        .Run(session)
                );
            Assert.That(
                error!.Message,
                Is.EqualTo("The right side of IN must evaluate to an array.")
            );
        }

        [Test]
        public void TestBracketElementAccessWithValueProperty() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE [\"EDID\"].value = 'TestArmor'"
            ).Run(session);

            MainRecord armor = plugin.records.Single(
                record => record.editorId == "TestArmor"
            );
            Assert.That(result.elements, Is.EqualTo(new[] { armor }));
        }

        [Test]
        public void TestBracketElementAccessWithSignatureProperty() {
            QueryResult result = Query.Parse(
                "SHOW records WHERE [\"EDID\"].signature = 'EDID'"
            ).Run(session);

            Assert.That(result.elements, Is.Not.Null);
            Assert.That(result.elements.Count, Is.GreaterThan(0));
        }

        private static IEnumerable<Element> descendants(Element element) {
            if (element is not Container container) yield break;
            foreach (Element child in container.elements) {
                yield return child;
                foreach (Element descendant in descendants(child))
                    yield return descendant;
            }
        }

        private object? AggregateValue(string aggregate) {
            QueryResult result = Query.Parse(
                $"AGGREGATE records {aggregate} fileFormId"
            ).Run(session);
            return ((ValueNode)result.root.children.Single()).value;
        }
    }
}
