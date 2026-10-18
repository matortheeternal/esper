#nullable enable

using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class EqualsOperator : QueryComparison {
        public override int precedence => 3;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^(?:==|=)\s*");
        }

        public static EqualsOperator Parse(Match match, string str) {
            return new EqualsOperator(match, str);
        }

        public EqualsOperator(Match match, string str) : base(match, str) { }

        public override object Evaluate(Element element) {
            return Matches(EvaluateLeft(element), EvaluateRight(element));
        }

        internal override bool Matches(object? left, object? right) {
            return QueryValue.equal(left, right);
        }
    }
}
