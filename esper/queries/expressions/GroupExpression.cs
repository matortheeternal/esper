#nullable enable

using esper.elements;
using System;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class GroupExpression : Expression {
        public Expression expression { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^\(\s*");
        }

        public static GroupExpression Parse(Match match, string str) {
            return new GroupExpression(match, str);
        }

        public GroupExpression(Match match, string str) : base(match, str) {
            expression = Expression.Parse(remainingStr);
            remainingStr = expression.remainingStr;
            Match close = Regex.Match(remainingStr, @"^\)\s*");
            if (!close.Success)
                throw new FormatException("Unterminated grouped expression.");
            remainingStr = remainingStr.Substring(close.Length);
        }

        public override object? Evaluate(Element element) {
            return expression.Evaluate(element);
        }
    }
}
