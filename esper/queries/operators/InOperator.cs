#nullable enable

using esper.elements;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class InOperator : QueryOperator {
        public override int precedence => 3;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^IN\b\s*", RegexOptions.IgnoreCase);
        }

        public static InOperator Parse(Match match, string str) {
            return new InOperator(match, str);
        }

        public InOperator(Match match, string str) : base(match, str) { }

        public override object Evaluate(Element element) {
            object? candidate = EvaluateLeft(element);
            object? values = EvaluateRight(element);
            if (values is object?[] array)
                return array.Any(value => QueryValue.equal(candidate, value));
            throw new InvalidOperationException(
                "The right side of IN must evaluate to an array."
            );
        }
    }
}
