#nullable enable

using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public abstract class QueryOperator : Expression {
        internal static readonly NodeParser<QueryOperator>[] parsers = [
            new(NotEqualsOperator.Matches, NotEqualsOperator.Parse),
            new(LessThanOperator.Matches, LessThanOperator.Parse),
            new(GreaterThanOperator.Matches, GreaterThanOperator.Parse),
            new(EqualsOperator.Matches, EqualsOperator.Parse),
            new(InOperator.Matches, InOperator.Parse),
            new(MatchesOperator.Matches, MatchesOperator.Parse),
            new(ContainsOperator.Matches, ContainsOperator.Parse),
            new(AndOperator.Matches, AndOperator.Parse),
            new(OrOperator.Matches, OrOperator.Parse)
        ];

        public Expression left { get; internal set; } = null!;
        public Expression right { get; internal set; } = null!;
        public abstract int precedence { get; }

        protected QueryOperator(Match match, string str) : base(match, str) { }

        protected object? EvaluateLeft(Element element) {
            return left.Evaluate(element);
        }

        protected object? EvaluateRight(Element element) {
            return right.Evaluate(element);
        }

        protected int compareOperands(Element element) {
            return QueryValue.Compare(
                EvaluateLeft(element),
                EvaluateRight(element)
            );
        }
    }

    public abstract class QueryComparison : QueryOperator {
        protected QueryComparison(Match match, string str) : base(match, str) { }

        internal abstract bool Matches(object? left, object? right);
    }
}
