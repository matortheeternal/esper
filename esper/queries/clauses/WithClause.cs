using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class WithClause : QueryClause {
        public QueryAggregate aggregate { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^WITH\b\s*", RegexOptions.IgnoreCase);
        }

        public static WithClause Parse(Match match, string str) {
            return new WithClause(match, str);
        }

        public WithClause(Match match, string str) : base(match, str) {
            aggregate = ParseFirst(QueryAggregate.parsers);
        }
    }
}
