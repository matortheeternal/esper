using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class AndOperator : QueryOperator {
        public override int precedence => 2;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^AND\b\s*", RegexOptions.IgnoreCase);
        }

        public static AndOperator Parse(Match match, string str) {
            return new AndOperator(match, str);
        }

        public AndOperator(Match match, string str) : base(match, str) { }

        public override object Evaluate(Element element) {
            return left.EvaluateCondition(element)
                && right.EvaluateCondition(element);
        }
    }
}
