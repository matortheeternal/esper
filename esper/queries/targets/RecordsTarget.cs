using esper.elements;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class RecordsTarget : QueryTarget {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^records\b\s*", RegexOptions.IgnoreCase);
        }

        public static RecordsTarget Parse(Match match, string str) {
            return new RecordsTarget(match, str);
        }

        public RecordsTarget(Match match, string str) : base(match, str) { }

        public override IEnumerable<Element> Select(IEnumerable<Element> source) {
            return source.OfType<MainRecord>();
        }
    }
}
