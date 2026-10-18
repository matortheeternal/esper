using esper.elements;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class StringExpression : Expression {
        public string value { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, "^'");
        }

        public static StringExpression Parse(Match match, string str) {
            return new StringExpression(match, str);
        }

        public StringExpression(Match match, string str) : base(match, str) {
            var parsed = new StringBuilder();
            int index = 0;
            while (index < remainingStr.Length) {
                char current = remainingStr[index];
                if (current != '\'') {
                    parsed.Append(current);
                    index++;
                    continue;
                }

                bool escaped = index + 1 < remainingStr.Length
                    && remainingStr[index + 1] == '\'';
                if (escaped) {
                    parsed.Append('\'');
                    index += 2;
                    continue;
                }

                value = parsed.ToString();
                remainingStr = remainingStr.Substring(index + 1);
                consumeWhitespace();
                return;
            }
            throw new FormatException("Unterminated string expression.");
        }

        public override object Evaluate(Element element) {
            return value;
        }
    }
}
