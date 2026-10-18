using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class OrOperator : QueryOperator {
        public override int precedence => 1;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^OR\b\s*", RegexOptions.IgnoreCase);
        }

        public static OrOperator Parse(Match match, string str) {
            return new OrOperator(match, str);
        }

        public OrOperator(Match match, string str) : base(match, str) { }

        public override object Evaluate(Element element) {
            return left.EvaluateCondition(element)
                || right.EvaluateCondition(element);
        }
    }
}
