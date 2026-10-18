using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class MatchesOperator : QueryOperator {
        public override int precedence => 3;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^MATCHES\b\s*", RegexOptions.IgnoreCase);
        }

        public static MatchesOperator Parse(Match match, string str) {
            return new MatchesOperator(match, str);
        }

        public MatchesOperator(Match match, string str) : base(match, str) { }

        public override object Evaluate(Element element) {
            string input = QueryValue.text(EvaluateLeft(element), "MATCHES");
            string pattern = QueryValue.text(EvaluateRight(element), "MATCHES");
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }
    }
}
