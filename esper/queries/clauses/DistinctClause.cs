using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class DistinctClause : QueryClause {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^DISTINCT\b\s*", RegexOptions.IgnoreCase);
        }

        public static DistinctClause Parse(Match match, string str) {
            return new DistinctClause(match, str);
        }

        public DistinctClause(Match match, string str) : base(match, str) { }
    }
}
