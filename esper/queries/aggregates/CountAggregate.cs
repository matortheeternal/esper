#nullable enable

using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class CountAggregate : QueryAggregate {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^COUNT\b\s*", RegexOptions.IgnoreCase);
        }

        public static CountAggregate Parse(Match match, string str) {
            return new CountAggregate(match, str);
        }

        public CountAggregate(Match match, string str) : base(match, str) {
            SetText();
        }

        public override object? Evaluate(IEnumerable<Element> elements) {
            return elements.Count();
        }
    }
}
