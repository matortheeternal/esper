#nullable enable

using esper.queries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.queries {
    public class QueryParserTests {
        [TestCase("SHOW records")]
        [TestCase("show records")]
        [TestCase("ShOw records;")]
        public void TestShowDispatchIgnoresCase(string text) {
            Assert.That(Query.Parse(text), Is.TypeOf<ShowQuery>());
        }

        [Test]
        public void TestAggregateDispatch() {
            var query = (AggregateQuery)Query.Parse(
                "AGGREGATE records SUM fileFormId"
            );
            Assert.That(query.aggregate, Is.TypeOf<SumAggregate>());
        }

        [Test]
        public void TestDistinctValuesDispatch() {
            var query = (ShowQuery)Query.Parse(
                "SHOW DISTINCT values WHERE signature = 'EDID' WITH COUNT"
            );

            Assert.That(query.distinct, Is.True);
            Assert.That(query.groupBy, Has.Count.EqualTo(1));
            Assert.That(query.groupBy[0].withClauses, Has.Count.EqualTo(1));
            Assert.That(
                query.groupBy[0].withClauses[0].aggregate,
                Is.TypeOf<CountAggregate>()
            );
        }

        [Test]
        public void TestDistinctRequiresValuesTarget() {
            Assert.Throws<FormatException>(() => Query.Parse("SHOW DISTINCT records"));
        }

        [Test]
        public void TestMutationDispatch() {
            var update = (UpdateQuery)Query.Parse(
                "UPDATE records SET editorId = 'Updated'"
            );
            Assert.That(update.setClause.property.name, Is.EqualTo("editorId"));
            Assert.That(Query.Parse("DELETE records"), Is.TypeOf<DeleteQuery>());
        }

        [Test]
        public void TestPartialStatementKeywordDoesNotMatch() {
            Assert.Throws<FormatException>(() => Query.Parse("SHOWCASE records"));
        }

        [TestCase("records", typeof(RecordsTarget))]
        [TestCase("elements", typeof(ElementsTarget))]
        [TestCase("values", typeof(ValuesTarget))]
        public void TestTargets(string text, Type targetType) {
            Query query = Query.Parse($"SHOW {text}");
            Assert.That(query.target, Is.TypeOf(targetType));
        }

        [TestCase("SHOW")]
        [TestCase("SHOW plugins")]
        public void TestMissingOrUnknownTargetIsRejected(string text) {
            Assert.Throws<FormatException>(() => Query.Parse(text));
        }

        [Test]
        public void TestSessionSourceDefaultsWhenAbsent() {
            Query query = Query.Parse("SHOW records");
            Assert.That(query.source, Is.TypeOf<SessionSource>());
        }

        [Test]
        public void TestExplicitSessionSource() {
            Query query = Query.Parse("SHOW records FROM session");
            Assert.That(query.source, Is.TypeOf<SessionSource>());
        }

        [Test]
        public void TestSelectionSource() {
            Query query = Query.Parse("SHOW records FROM selection 42");

            var source = (SelectionSource)query.source;
            Assert.That(source.selectionId, Is.EqualTo(42));
        }

        [Test]
        public void TestInvalidExplicitSourceIsRejectedAtSource() {
            FormatException? error = Assert.Throws<FormatException>(
                () => Query.Parse("SHOW records FROM selection invalid")
            );
            Assert.That(error!.Message, Does.StartWith("Invalid query source"));
        }

        [Test]
        public void TestComposedQuery() {
            Query query = Query.Parse(
                "SHOW records FROM session "
                + "WHERE signature = NPC_ "
                + "GROUP BY signature WITH COUNT "
                + "GROUP BY file WITH COUNT "
                + "ORDER BY formId;"
            );

            Assert.That(query.whereClause!.expression, Is.TypeOf<EqualsOperator>());
            Assert.That(query.groupBy, Has.Count.EqualTo(2));
            Assert.That(
                query.groupBy[0].withClauses[0].aggregate,
                Is.TypeOf<CountAggregate>()
            );
            Assert.That(
                ((PropertyExpression)query.groupBy[0].expression).name,
                Is.EqualTo("signature")
            );
            Assert.That(
                ((PropertyExpression)query.groupBy[1].expression).name,
                Is.EqualTo("file")
            );
            Assert.That(query.orderBy, Has.Count.EqualTo(1));
            Assert.That(query.remainingStr, Is.Empty);
        }

        [Test]
        public void TestBooleanExpressionHierarchy() {
            Query query = Query.Parse(
                "SHOW records WHERE signature = NPC_ "
                + "AND editorId CONTAINS 'Bandit'"
            );

            var conjunction = (AndOperator)query.whereClause!.expression;
            Assert.That(conjunction.left, Is.TypeOf<EqualsOperator>());
            Assert.That(conjunction.right, Is.TypeOf<ContainsOperator>());
            var contains = (ContainsOperator)conjunction.right;
            Assert.That(contains.right, Is.TypeOf<StringExpression>());
            Assert.That(
                ((StringExpression)contains.right).value,
                Is.EqualTo("Bandit")
            );
        }

        [Test]
        public void TestInArrayExpression() {
            Query query = Query.Parse(
                "SHOW records WHERE signature IN (NPC_, ARMO)"
            );
            var operation = (InOperator)query.whereClause!.expression;
            var array = (ArrayExpression)operation.right;
            Assert.That(array.values, Has.Count.EqualTo(2));
        }

        [TestCase("ARMO", "ARMO")]
        [TestCase("0A0B", "0A0B")]
        [TestCase("@IAD", "@IAD")]
        [TestCase(":0TX", ":0TX")]
        [TestCase(";0TX", ";0TX")]
        [TestCase("<0TX", "<0TX")]
        [TestCase("=0TX", "=0TX")]
        [TestCase(">0TX", ">0TX")]
        [TestCase("?0TX", "?0TX")]
        [TestCase("@0TX", "@0TX")]
        public void TestUnquotedSignaturesPreserveAllSignatureBytes(
            string literal,
            string expected
        ) {
            UnquotedSignatureExpression signature = SignatureExpression(literal);
            Assert.That(signature.value, Is.EqualTo(expected));
        }

        [Test]
        public void TestUnquotedSignaturesAcceptAllDeclaredControlPrefixes() {
            for (char prefix = '\0'; prefix <= '\x14'; prefix++) {
                string expected = $"{prefix}IAD";
                var raw = (UnquotedSignatureExpression)Expression.Parse(expected);
                UnquotedSignatureExpression escaped = SignatureExpression(
                    $@"\x{(int)prefix:X2}IAD"
                );

                Assert.That(raw.value, Is.EqualTo(expected));
                Assert.That(escaped.value, Is.EqualTo(expected));
            }
        }

        [Test]
        public void TestUnquotedSignaturesRejectLowercaseBytes() {
            Query query = Query.Parse("SHOW records WHERE signature = armo");
            var comparison = (EqualsOperator)query.whereClause!.expression;

            Assert.That(comparison.right, Is.TypeOf<PropertyExpression>());
            Assert.Throws<FormatException>(
                () => Query.Parse("SHOW records WHERE signature = \\x61IAD")
            );
        }

        private static UnquotedSignatureExpression SignatureExpression(
            string literal
        ) {
            Query query = Query.Parse(
                $"SHOW records WHERE signature = {literal}"
            );
            var comparison = (EqualsOperator)query.whereClause!.expression;
            return (UnquotedSignatureExpression)comparison.right;
        }

        [Test]
        public void TestLongerComparisonOperatorMatchesFirst() {
            Query query = Query.Parse("SHOW records WHERE formId >= 10");
            var operation = (GreaterThanOperator)query.whereClause!.expression;
            Assert.That(operation.inclusive, Is.True);
        }

        [TestCase("SHOW records garbage")]
        [TestCase("SHOW records;")]
        public void TestTrailingInputRules(string text) {
            if (text.EndsWith(';')) {
                Assert.DoesNotThrow(() => Query.Parse(text));
                return;
            }
            Assert.Throws<FormatException>(() => Query.Parse(text));
        }

        [TestCase("SHOW records WHERE")]
        [TestCase("SHOW records WHERE name =")]
        [TestCase("SHOW records WHERE name = 'unterminated")]
        [TestCase("SHOW records WHERE (name = value")]
        public void TestInvalidRecognizedConstructIsRejected(string text) {
            Assert.Throws<FormatException>(() => Query.Parse(text));
        }

        [Test]
        public void TestParseMultiplePropagatesRemainingSuffix() {
            var harness = new ParserHarness("xx-rest");
            IReadOnlyList<TestNode> nodes = harness.parseMany(consumingParser);
            Assert.That(nodes, Has.Count.EqualTo(2));
            Assert.That(harness.remainingStr, Is.EqualTo("-rest"));
        }

        [Test]
        public void TestNonConsumingParserIsRejected() {
            var harness = new ParserHarness("x");
            var parser = new NodeParser<TestNode>(
                str => Regex.Match(str, "^x"),
                (match, str) => new TestNode(str)
            );
            Assert.Throws<InvalidOperationException>(() => harness.Parse(parser));
        }

        [Test]
        public void TestUnanchoredParserIsRejected() {
            var harness = new ParserHarness("-x");
            var parser = new NodeParser<TestNode>(
                str => Regex.Match(str, "x"),
                (match, str) => new TestNode("")
            );
            Assert.Throws<InvalidOperationException>(() => harness.Parse(parser));
        }

        private static readonly NodeParser<TestNode> consumingParser = new(
            str => Regex.Match(str, "^x"),
            (match, str) => new TestNode(str.Substring(match.Length))
        );

        private sealed class ParserHarness : ASTNode {
            internal ParserHarness(string str) {
                remainingStr = str;
            }

            internal TestNode Parse(NodeParser<TestNode> parser) {
                return ParseOne(parser)!;
            }

            internal IReadOnlyList<TestNode> parseMany(
                NodeParser<TestNode> parser
            ) {
                return ParseMultiple(parser);
            }
        }

        private sealed class TestNode : ASTNode {
            internal TestNode(string str) {
                remainingStr = str;
            }
        }
    }
}
