#nullable enable

using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class GreaterThanOperator : QueryComparison {
        public bool inclusive { get; }
        public override int precedence => 3;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^(?:>=|>)\s*");
        }

        public static GreaterThanOperator Parse(Match match, string str) {
            return new GreaterThanOperator(match, str);
        }

        public GreaterThanOperator(Match match, string str) : base(match, str) {
            inclusive = match.Value.TrimStart().StartsWith(">=");
        }

        public override object Evaluate(Element element) {
            return Matches(EvaluateLeft(element), EvaluateRight(element));
        }

        internal override bool Matches(object? left, object? right) {
            int comparison = QueryValue.Compare(left, right);
            return inclusive ? comparison >= 0 : comparison > 0;
        }
    }
}
