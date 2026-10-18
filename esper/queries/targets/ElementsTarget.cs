using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class ElementsTarget : QueryTarget {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^elements\b\s*", RegexOptions.IgnoreCase);
        }

        public static ElementsTarget Parse(Match match, string str) {
            return new ElementsTarget(match, str);
        }

        public ElementsTarget(Match match, string str) : base(match, str) { }

        public override IEnumerable<Element> Select(IEnumerable<Element> source) {
            return source.SelectMany(descendants);
        }
    }
}
