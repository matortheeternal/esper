using esper.elements;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class OrderByClause : QueryClause {
        public Expression expression { get; }
        public bool descending { get; }

        public static Match Matches(string str) {
            return Regex.Match(
                str,
                @"^ORDER\s+BY\b\s*",
                RegexOptions.IgnoreCase
            );
        }

        public static OrderByClause Parse(Match match, string str) {
            return new OrderByClause(match, str);
        }

        public OrderByClause(Match match, string str) : base(match, str) {
            expression = Expression.Parse(remainingStr);
            remainingStr = expression.remainingStr;
            Match direction = Regex.Match(
                remainingStr,
                @"^(ASC|DESC)\b\s*",
                RegexOptions.IgnoreCase
            );
            if (!direction.Success) return;
            descending = direction.Groups[1].Value.Equals(
                "DESC",
                System.StringComparison.OrdinalIgnoreCase
            );
            remainingStr = remainingStr.Substring(direction.Length);
        }

        public int Compare(Element left, Element right) {
            int comparison = QueryValue.Compare(
                expression.Evaluate(left),
                expression.Evaluate(right)
            );
            return descending ? -comparison : comparison;
        }
    }
}
