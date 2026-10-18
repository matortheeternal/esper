#nullable enable

using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class SumAggregate : NumericAggregate {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^SUM\b\s*", RegexOptions.IgnoreCase);
        }

        public static SumAggregate Parse(Match match, string str) {
            return new SumAggregate(match, str);
        }

        public SumAggregate(Match match, string str) : base(match, str) { }

        public override object? Evaluate(IEnumerable<Element> elements) {
            return Values(elements, "SUM").Sum();
        }
    }
}
