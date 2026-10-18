#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using esper.elements;
using esper.setup;

namespace esper.queries {
    public abstract class Query : ASTNode {
        private static readonly NodeParser<Query>[] queryParsers = [
            new(ShowQuery.Matches, ShowQuery.Parse),
            new(AggregateQuery.Matches, AggregateQuery.Parse),
            new(UpdateQuery.Matches, UpdateQuery.Parse),
            new(DeleteQuery.Matches, DeleteQuery.Parse)
        ];

        private static readonly NodeParser<QueryTarget>[] targetParsers = [
            new(RecordsTarget.Matches, RecordsTarget.Parse),
            new(ElementsTarget.Matches, ElementsTarget.Parse),
            new(ValuesTarget.Matches, ValuesTarget.Parse)
        ];

        private static readonly NodeParser<QuerySource>[] sourceParsers = [
            new(SessionSource.Matches, SessionSource.Parse),
            new(SelectionSource.Matches, SelectionSource.Parse)
        ];

        private static readonly NodeParser<WhereClause> whereParser =
            new(WhereClause.Matches, WhereClause.Parse);
        private static readonly NodeParser<GroupByClause> groupByParser =
            new(GroupByClause.Matches, GroupByClause.Parse);
        private static readonly NodeParser<OrderByClause> orderByParser =
            new(OrderByClause.Matches, OrderByClause.Parse);

        public string q { get; }
        public QueryTarget target { get; private set; } = null!;
        public QuerySource source { get; private set; } = null!;
        public WhereClause? whereClause { get; private set; }
        public IReadOnlyList<GroupByClause> groupBy { get; private set; } = [];
        public IReadOnlyList<OrderByClause> orderBy { get; private set; } = [];

        protected Query(Match match, string str) : base(match, str) {
            q = str;
        }

        public static Query Parse(string str) {
            ArgumentNullException.ThrowIfNull(str);
            var root = new QueryRoot(str);
            Query query = root.ParseQuery();
            Match end = Regex.Match(query.remainingStr, @"^\s*;?\s*$");
            if (end.Success) {
                query.remainingStr = "";
                return query;
            }

            int offset = str.Length - query.remainingStr.Length;
            throw new FormatException(
                $"Unexpected query input at offset {offset}: {query.remainingStr}"
            );
        }

        public QueryResult Run(Session session) {
            ArgumentNullException.ThrowIfNull(session);
            return Execute(session);
        }

        protected abstract QueryResult Execute(Session session);

        protected void ParseComponents() {
            ParseTargetAndSource();
            ParseWhereClause();
            ParseGroupingAndOrdering();
        }

        protected void ParseGroupingAndOrdering() {
            groupBy = ParseMultiple(groupByParser, true);
            ParseOrderBy();
        }

        protected void ParseOrderBy() {
            orderBy = ParseMultiple(orderByParser, true);
        }

        protected void SetGroupBy(IReadOnlyList<GroupByClause> clauses) {
            groupBy = clauses;
        }

        protected void ParseTargetAndSource() {
            target = ParseFirst(targetParsers)!;
            source = ParseSource();
        }

        protected void ParseWhereClause() {
            whereClause = ParseOne(whereParser, true);
        }

        protected Element[] Select(Session session) {
            IEnumerable<Element> selected = target.Select(source.Select(session));
            if (whereClause != null) selected = whereClause.Apply(selected);
            return selected.ToArray();
        }

        private QuerySource ParseSource() {
            QuerySource? parsed = ParseFirst(sourceParsers, true);
            if (parsed != null) return parsed;
            if (Regex.IsMatch(remainingStr, @"^FROM\b", RegexOptions.IgnoreCase))
                throw new FormatException($"Invalid query source at: {remainingStr}");
            return SessionSource.defaultFor(remainingStr);
        }

        private sealed class QueryRoot : ASTNode {
            internal QueryRoot(string str) {
                remainingStr = str;
            }

            internal Query ParseQuery() {
                return ParseFirst(queryParsers)!;
            }
        }
    }
}
