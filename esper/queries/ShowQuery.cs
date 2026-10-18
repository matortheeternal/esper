#nullable enable

using esper.elements;
using esper.setup;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class ShowQuery : Query {
        private static readonly NodeParser<DistinctClause> distinctParser =
            new(DistinctClause.Matches, DistinctClause.Parse);
        private static readonly NodeParser<WithClause> withParser =
            new(WithClause.Matches, WithClause.Parse);

        public bool distinct { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^\s*SHOW\b\s*", RegexOptions.IgnoreCase);
        }

        public static ShowQuery Parse(Match match, string str) {
            return new ShowQuery(match, str);
        }

        public ShowQuery(Match match, string str) : base(match, str) {
            distinct = ParseOne(distinctParser, true) != null;
            ParseTargetAndSource();
            ParseWhereClause();
            if (distinct) ParseDistinctGrouping();
            if (!distinct) ParseGroupingAndOrdering();
            if (distinct) ParseDistinctOrdering();
        }

        protected override QueryResult Execute(Session session) {
            IEnumerable<Element> selected = source.Select(session);
            selected = ShowCandidateStrategies.Apply(this, session, selected);
            selected = target.Select(selected);
            if (whereClause != null) selected = whereClause.Apply(selected);
            selected = ApplyOrderBy(selected);
            return new QueryResult(selected.ToArray(), groupBy);
        }

        private IEnumerable<Element> ApplyOrderBy(IEnumerable<Element> selected) {
            if (orderBy.Count == 0) return selected;
            return selected.OrderBy(
                element => element,
                new QueryElementComparer(orderBy)
            );
        }

        private void ParseDistinctGrouping() {
            if (target is not ValuesTarget)
                throw new FormatException("SHOW DISTINCT requires the values target.");
            var withClauses = ParseMultiple(withParser, true);
            SetGroupBy([GroupByClause.ForDistinctValues(withClauses)]);
        }

        private void ParseDistinctOrdering() {
            if (GroupByClause.Matches(remainingStr).Success)
                throw new FormatException("SHOW DISTINCT cannot use GROUP BY.");
            ParseOrderBy();
        }

    }
}
