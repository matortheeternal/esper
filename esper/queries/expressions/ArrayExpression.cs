#nullable enable

using esper.elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class ArrayExpression : Expression {
        public IReadOnlyList<Expression> values { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^\(\s*(?=[^()]*,)");
        }

        public static ArrayExpression Parse(Match match, string str) {
            return new ArrayExpression(match, str);
        }

        public ArrayExpression(Match match, string str) : base(match, str) {
            var parsed = new List<Expression>();
            while (true) {
                Expression value = Expression.Parse(remainingStr);
                parsed.Add(value);
                remainingStr = value.remainingStr;

                Match close = Regex.Match(remainingStr, @"^\)\s*");
                if (close.Success) {
                    remainingStr = remainingStr.Substring(close.Length);
                    values = parsed;
                    return;
                }

                Match comma = Regex.Match(remainingStr, @"^,\s*");
                if (!comma.Success)
                    throw new FormatException("Expected ',' or ')' in array expression.");
                remainingStr = remainingStr.Substring(comma.Length);
            }
        }

        public override object? Evaluate(Element element) {
            return values.Select(value => value.Evaluate(element)).ToArray();
        }
    }
}
