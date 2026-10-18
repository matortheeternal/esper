#nullable enable

using esper.elements;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class ContainsOperator : QueryOperator {
        public override int precedence => 3;

        public static Match Matches(string str) {
            return Regex.Match(str, @"^CONTAINS\b\s*", RegexOptions.IgnoreCase);
        }

        public static ContainsOperator Parse(Match match, string str) {
            return new ContainsOperator(match, str);
        }

        public ContainsOperator(Match match, string str) : base(match, str) { }

        public override object Evaluate(Element element) {
            object? container = EvaluateLeft(element);
            object? candidate = EvaluateRight(element);
            if (container is string text) {
                string value = QueryValue.text(candidate, "CONTAINS");
                return text.Contains(value, StringComparison.OrdinalIgnoreCase);
            }
            if (container is IEnumerable enumerable)
                return enumerable.Cast<object?>().Any(
                    value => QueryValue.equal(value, candidate)
                );
            throw new InvalidOperationException(
                "The left side of CONTAINS must be text or an array."
            );
        }
    }
}
