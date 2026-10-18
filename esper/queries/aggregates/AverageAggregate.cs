#nullable enable

using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class AverageAggregate : NumericAggregate {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^AVG\b\s*", RegexOptions.IgnoreCase);
        }

        public static AverageAggregate Parse(Match match, string str) {
            return new AverageAggregate(match, str);
        }

        public AverageAggregate(Match match, string str) : base(match, str) { }

        public override object? Evaluate(IEnumerable<Element> elements) {
            decimal[] values = Values(elements, "AVG");
            if (values.Length == 0) return null;
            return values.Average();
        }
    }
}
