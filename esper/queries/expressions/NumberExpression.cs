using esper.elements;
using System.Globalization;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class NumberExpression : Expression {
        public decimal value { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^-?(?:\d+(?:\.\d+)?|\.\d+)\b\s*");
        }

        public static NumberExpression Parse(Match match, string str) {
            return new NumberExpression(match, str);
        }

        public NumberExpression(Match match, string str) : base(match, str) {
            value = decimal.Parse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        public override object Evaluate(Element element) {
            return value;
        }
    }
}
