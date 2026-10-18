#nullable enable

using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class MinAggregate : NumericAggregate {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^MIN\b\s*", RegexOptions.IgnoreCase);
        }

        public static MinAggregate Parse(Match match, string str) {
            return new MinAggregate(match, str);
        }

        public MinAggregate(Match match, string str) : base(match, str) { }

        public override object? Evaluate(IEnumerable<Element> elements) {
            decimal[] values = Values(elements, "MIN");
            if (values.Length == 0) return null;
            return values.Min();
        }
    }
}
