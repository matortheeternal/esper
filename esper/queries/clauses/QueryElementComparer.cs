#nullable enable

using esper.elements;
using System.Collections.Generic;

namespace esper.queries {
    internal sealed class QueryElementComparer : IComparer<Element> {
        private readonly IReadOnlyList<OrderByClause> clauses;

        internal QueryElementComparer(IReadOnlyList<OrderByClause> clauses) {
            this.clauses = clauses;
        }

        public int Compare(Element? left, Element? right) {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            foreach (OrderByClause clause in clauses) {
                int comparison = clause.Compare(left, right);
                if (comparison != 0) return comparison;
            }
            return 0;
        }
    }
}
