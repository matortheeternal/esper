#nullable enable

using esper.elements;
using System.Collections.Generic;
using System.Linq;

namespace esper.queries {
    public sealed class QueryResult {
        public IReadOnlyList<Element> elements { get; }
        public TreeNode root { get; }

        internal QueryResult(
            IReadOnlyList<Element> elements,
            IReadOnlyList<GroupByClause>? groupBy = null
        ) {
            this.elements = elements;
            root = new RootNode(this, () => CreateChildren(elements, groupBy ?? []));
        }

        internal static QueryResult CreateValues(IReadOnlyList<object?> values) {
            return new QueryResult(values);
        }

        private QueryResult(IReadOnlyList<object?> values) {
            elements = [];
            root = new RootNode(this, () => values.Select(
                value => (TreeNode)new ValueNode(this, value)
            ).ToArray());
        }

        private IReadOnlyList<TreeNode> CreateChildren(
            IReadOnlyList<Element> source,
            IReadOnlyList<GroupByClause> groupBy,
            int level = 0
        ) {
            if (level == groupBy.Count)
                return source.Select(element => (TreeNode)new ElementNode(this, element))
                    .ToArray();

            GroupByClause clause = groupBy[level];
            var keys = new List<GroupKey>();
            var elementsByKey = new Dictionary<GroupKey, List<Element>>();
            foreach (Element element in source) {
                var key = new GroupKey(clause.expression.Evaluate(element));
                if (!elementsByKey.TryGetValue(key, out List<Element>? children)) {
                    children = [];
                    elementsByKey.Add(key, children);
                    keys.Add(key);
                }
                children.Add(element);
            }
            return keys.Select(key => (TreeNode)new GroupNode(
                this,
                clause.expression,
                key.value,
                elementsByKey[key],
                EvaluateAggregates(clause, elementsByKey[key]),
                () => CreateChildren(elementsByKey[key], groupBy, level + 1)
            )).ToArray();
        }

        private static Dictionary<string, object?> EvaluateAggregates(
            GroupByClause clause,
            IReadOnlyList<Element> elements
        ) {
            return clause.withClauses.ToDictionary(
                withClause => withClause.aggregate.text,
                withClause => withClause.aggregate.Evaluate(elements)
            );
        }

        private readonly struct GroupKey : IEquatable<GroupKey> {
            internal object? value { get; }
            private object? comparableValue { get; }
            private string? text { get; }
            private decimal? number { get; }

            internal GroupKey(object? value) {
                this.value = value;
                comparableValue = QueryValue.Unwrap(value);
                text = comparableValue as string;
                if (text != null) return;
                if (QueryValue.TryGetNumber(comparableValue, out decimal valueNumber))
                    number = valueNumber;
            }

            public bool Equals(GroupKey other) {
                if (text != null || other.text != null)
                    return text != null
                        && other.text != null
                        && text.Equals(other.text, System.StringComparison.OrdinalIgnoreCase);
                if (number.HasValue || other.number.HasValue)
                    return number == other.number;
                return object.Equals(comparableValue, other.comparableValue);
            }

            public override bool Equals(object? obj) {
                return obj is GroupKey other && Equals(other);
            }

            public override int GetHashCode() {
                if (text != null)
                    return System.StringComparer.OrdinalIgnoreCase.GetHashCode(text);
                if (number.HasValue) return number.Value.GetHashCode();
                return comparableValue?.GetHashCode() ?? 0;
            }
        }
    }
}
