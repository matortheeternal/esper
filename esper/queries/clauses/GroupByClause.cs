#nullable enable

using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class GroupByClause : QueryClause {
        private static readonly NodeParser<WithClause> withParser =
            new(WithClause.Matches, WithClause.Parse);

        public Expression expression { get; }
        public IReadOnlyList<WithClause> withClauses { get; }

        public static Match Matches(string str) {
            return Regex.Match(
                str,
                @"^GROUP\s+BY\b\s*",
                RegexOptions.IgnoreCase
            );
        }

        public static GroupByClause Parse(Match match, string str) {
            return new GroupByClause(match, str);
        }

        internal static GroupByClause ForDistinctValues(
            IReadOnlyList<WithClause> withClauses
        ) {
            return new GroupByClause(Expression.Parse("value"), withClauses);
        }

        public GroupByClause(Match match, string str) : base(match, str) {
            expression = Expression.Parse(remainingStr);
            remainingStr = expression.remainingStr;
            withClauses = ParseMultiple(withParser, true);
        }

        private GroupByClause(
            Expression expression,
            IReadOnlyList<WithClause> withClauses
        ) {
            this.expression = expression;
            this.withClauses = withClauses;
        }
    }
}
