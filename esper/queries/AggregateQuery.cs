using esper.setup;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class AggregateQuery : Query {
        public QueryAggregate aggregate { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^\s*AGGREGATE\b\s*", RegexOptions.IgnoreCase);
        }

        public static AggregateQuery Parse(Match match, string str) {
            return new AggregateQuery(match, str);
        }

        public AggregateQuery(Match match, string str) : base(match, str) {
            ParseTargetAndSource();
            aggregate = ParseFirst(QueryAggregate.parsers)!;
            ParseWhereClause();
        }

        protected override QueryResult Execute(Session session) {
            return QueryResult.CreateValues([aggregate.Evaluate(Select(session))]);
        }
    }
}
