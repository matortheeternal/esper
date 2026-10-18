#nullable enable

using esper.elements;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class FunctionExpression : Expression {
        public string name { get; }
        public IReadOnlyList<Expression> arguments { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^([A-Za-z_][A-Za-z0-9_.]*)\s*\(\s*");
        }

        public static FunctionExpression Parse(Match match, string str) {
            return new FunctionExpression(match, str);
        }

        public FunctionExpression(Match match, string str) : base(match, str) {
            name = match.Groups[1].Value;
            var parsed = new List<Expression>();
            Match empty = Regex.Match(remainingStr, @"^\)\s*");
            if (empty.Success) {
                remainingStr = remainingStr.Substring(empty.Length);
                arguments = parsed;
                return;
            }

            while (true) {
                Expression argument = Expression.Parse(remainingStr);
                parsed.Add(argument);
                remainingStr = argument.remainingStr;

                Match close = Regex.Match(remainingStr, @"^\)\s*");
                if (close.Success) {
                    remainingStr = remainingStr.Substring(close.Length);
                    arguments = parsed;
                    return;
                }

                Match comma = Regex.Match(remainingStr, @"^,\s*");
                if (!comma.Success) {
                    throw new FormatException(
                        "Expected ',' or ')' in function expression."
                    );
                }
                remainingStr = remainingStr.Substring(comma.Length);
            }
        }

        public override object? Evaluate(Element element) {
            throw new NotSupportedException(
                $"Query function '{name}' cannot be executed."
            );
        }
    }
}
