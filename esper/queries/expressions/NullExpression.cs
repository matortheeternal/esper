#nullable enable

using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class NullExpression : Expression {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^NULL\b\s*", RegexOptions.IgnoreCase);
        }

        public static NullExpression Parse(Match match, string str) {
            return new NullExpression(match, str);
        }

        public NullExpression(Match match, string str) : base(match, str) { }

        public override object? Evaluate(Element element) {
            return null;
        }
    }
}
