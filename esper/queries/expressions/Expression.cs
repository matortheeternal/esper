#nullable enable

using System;
using System.Text.RegularExpressions;
using esper.elements;

namespace esper.queries {
    public abstract class Expression : ASTNode {
        protected Expression(Match match, string str) : base(match, str) { }

        public abstract object? Evaluate(Element element);

        public bool EvaluateCondition(Element element) {
            return QueryValue.condition(Evaluate(element));
        }

        internal static Expression Parse(string str) {
            return new ExpressionReader(str).ParseExpression();
        }

        internal static Expression Parse(string str, int minimumPrecedence) {
            return new ExpressionReader(str).ParseExpression(minimumPrecedence);
        }

        private sealed class ExpressionReader : ASTNode {
            private static readonly NodeParser<Expression>[] primaryParsers = [
                new(StringExpression.Matches, StringExpression.Parse),
                new(NumberExpression.Matches, NumberExpression.Parse),
                new(UnquotedSignatureExpression.Matches, UnquotedSignatureExpression.Parse),
                new(NullExpression.Matches, NullExpression.Parse),
                new(FunctionExpression.Matches, FunctionExpression.Parse),
                new(ArrayExpression.Matches, ArrayExpression.Parse),
                new(GroupExpression.Matches, GroupExpression.Parse),
                new(NotOperator.Matches, NotOperator.Parse),
                new(PropertyExpression.Matches, PropertyExpression.Parse)
            ];

            internal ExpressionReader(string str) {
                remainingStr = str;
            }

            internal Expression ParseExpression(int minimumPrecedence = 0) {
                Expression left = ParseFirst(primaryParsers)!;
                while (true) {
                    QueryOperator? operation = ParseOperator(minimumPrecedence);
                    if (operation == null) return left;
                    operation.left = left;
                    operation.right = ParseExpression(operation.precedence + 1);
                    operation.remainingStr = operation.right.remainingStr;
                    remainingStr = operation.remainingStr;
                    left = operation;
                }
            }

            private QueryOperator? ParseOperator(int minimumPrecedence) {
                foreach (NodeParser<QueryOperator> parser in QueryOperator.parsers) {
                    Match match = parser.Matches(remainingStr);
                    if (!match.Success) continue;
                    QueryOperator operation = parser.Parse(match, remainingStr);
                    if (operation.precedence < minimumPrecedence) return null;
                    remainingStr = operation.remainingStr;
                    return operation;
                }
                return null;
            }
        }
    }
}
