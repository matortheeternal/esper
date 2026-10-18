#nullable enable

using esper.elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class WhereClause : QueryClause {
        public Expression expression { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^WHERE\b\s*", RegexOptions.IgnoreCase);
        }

        public static WhereClause Parse(Match match, string str) {
            return new WhereClause(match, str);
        }

        public WhereClause(Match match, string str) : base(match, str) {
            expression = Expression.Parse(remainingStr);
            remainingStr = expression.remainingStr;
        }

        public IEnumerable<Element> Apply(IEnumerable<Element> elements) {
            return elements.Where(Matches);
        }

        public bool Matches(Element element) {
            return expression.EvaluateCondition(element);
        }

        internal bool TryGetRecordFilter(out Func<MainRecord, bool> filter) {
            return TryGetRecordFilter(false, out filter);
        }

        internal bool TryGetRecordScopedFilter(out Func<MainRecord, bool> filter) {
            return TryGetRecordFilter(true, out filter);
        }

        private bool TryGetRecordFilter(
            bool requireRecordScope,
            out Func<MainRecord, bool> filter
        ) {
            filter = null!;
            if (expression is not QueryOperator op) return false;
            if (op is AndOperator) {
                if (TrySideAsFilter(op.left, requireRecordScope, out filter)) return true;
                if (TrySideAsFilter(op.right, requireRecordScope, out filter)) return true;
                return false;
            }
            return TrySideAsFilter(op, requireRecordScope, out filter);
        }

        internal bool TryGetComparison(
            string propertyName,
            out QueryComparison comparison,
            out object? value,
            out bool propertyFirst
        ) {
            foreach (Expression condition in Conjuncts(expression)) {
                if (condition is not QueryComparison current) continue;
                if (TryComparison(
                    current.left,
                    current.right,
                    propertyName,
                    out value
                )) {
                    comparison = current;
                    propertyFirst = true;
                    return true;
                }
                if (TryComparison(
                    current.right,
                    current.left,
                    propertyName,
                    out value
                )) {
                    comparison = current;
                    propertyFirst = false;
                    return true;
                }
            }
            comparison = null!;
            value = null;
            propertyFirst = false;
            return false;
        }

        private static bool TrySideAsFilter(
            Expression side,
            bool requireRecordScope,
            out Func<MainRecord, bool> filter
        ) {
            filter = null!;
            if (side is not QueryOperator op) return false;
            if (op.left is not PropertyExpression leftProp) return false;
            if (!IsRecordProperty(leftProp.name, requireRecordScope)) return false;
            if (!(op.right is StringExpression or NumberExpression or NullExpression)) return false;
            filter = record => op.Evaluate(record) is bool b && b;
            return true;
        }

        private static IEnumerable<Expression> Conjuncts(Expression expression) {
            if (expression is not AndOperator and) return [expression];
            return Conjuncts(and.left).Concat(Conjuncts(and.right));
        }

        private static bool TryComparison(
            Expression property,
            Expression value,
            string propertyName,
            out object? literal
        ) {
            literal = null;
            if (property is not PropertyExpression propertyExpression) return false;
            if (!propertyExpression.name.Equals(
                propertyName,
                StringComparison.OrdinalIgnoreCase
            )) return false;
            return TryLiteral(value, out literal);
        }

        private static bool TryLiteral(Expression expression, out object? value) {
            if (expression is StringExpression text) {
                value = text.value;
                return true;
            }
            if (expression is UnquotedSignatureExpression signature) {
                value = signature.value;
                return true;
            }
            if (expression is NumberExpression number) {
                value = number.value;
                return true;
            }
            if (expression is NullExpression) {
                value = null;
                return true;
            }
            value = null;
            return false;
        }

        private static readonly HashSet<string> recordHeaderProperties = new(
            StringComparer.OrdinalIgnoreCase
        ) {
            "signature", "formId", "fileFormId", "localFormId", "globalFormId",
            "local", "compressed", "dataSize"
        };

        private static bool IsRecordProperty(string name, bool requireRecordScope) {
            var parts = name.Split('.');
            if (parts.Length == 2
                && parts[0].Equals("record", StringComparison.OrdinalIgnoreCase))
                return recordHeaderProperties.Contains(parts[1]);
            if (requireRecordScope) return false;
            return recordHeaderProperties.Contains(name);
        }
    }
}
