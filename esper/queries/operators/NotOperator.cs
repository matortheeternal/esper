using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class NotOperator : Expression {
        public Expression expression { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^NOT\b\s*", RegexOptions.IgnoreCase);
        }

        public static NotOperator Parse(Match match, string str) {
            return new NotOperator(match, str);
        }

        public NotOperator(Match match, string str) : base(match, str) {
            expression = Expression.Parse(remainingStr, 3);
            remainingStr = expression.remainingStr;
        }

        public override object Evaluate(Element element) {
            return !expression.EvaluateCondition(element);
        }
    }
}
