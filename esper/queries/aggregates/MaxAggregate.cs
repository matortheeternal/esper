#nullable enable

using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class MaxAggregate : NumericAggregate {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^MAX\b\s*", RegexOptions.IgnoreCase);
        }

        public static MaxAggregate Parse(Match match, string str) {
            return new MaxAggregate(match, str);
        }

        public MaxAggregate(Match match, string str) : base(match, str) { }

        public override object? Evaluate(IEnumerable<Element> elements) {
            decimal[] values = Values(elements, "MAX");
            if (values.Length == 0) return null;
            return values.Max();
        }
    }
}
