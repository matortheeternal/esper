using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class ValuesTarget : QueryTarget {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^values\b\s*", RegexOptions.IgnoreCase);
        }

        public static ValuesTarget Parse(Match match, string str) {
            return new ValuesTarget(match, str);
        }

        public ValuesTarget(Match match, string str) : base(match, str) { }

        public override IEnumerable<Element> Select(IEnumerable<Element> source) {
            return source.SelectMany(descendants).OfType<ValueElement>();
        }
    }
}
