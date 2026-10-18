#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.queries {
    public abstract class ASTNode {
        internal string remainingStr = "";

        protected ASTNode() { }

        protected ASTNode(Match match, string str) {
            if (!match.Success || match.Index != 0 || match.Length == 0) {
                throw new InvalidOperationException(
                    "A node match must consume the input prefix."
                );
            }
            remainingStr = str.Substring(match.Length);
        }

        internal T? ParseFirst<T>(
            IEnumerable<NodeParser<T>> allowed,
            bool allowNull = false
        ) where T : ASTNode {
            foreach (NodeParser<T> parser in allowed) {
                Match match = parser.Matches(remainingStr);
                if (!match.Success) continue;
                if (match.Index != 0) {
                    throw new InvalidOperationException(
                        $"A {typeof(T).Name} match was not anchored."
                    );
                }

                string input = remainingStr;
                T node = parser.Parse(match, input);
                validateProgress(node, input);
                remainingStr = node.remainingStr;
                return node;
            }

            if (allowNull) return null;
            throw new FormatException($"Expected {typeof(T).Name} at: {remainingStr}");
        }

        internal T? ParseOne<T>(
            NodeParser<T> parser,
            bool allowNull = false
        ) where T : ASTNode {
            return ParseFirst([parser], allowNull);
        }

        internal List<T> ParseMultiple<T>(
            NodeParser<T> parser,
            bool allowNull = false
        ) where T : ASTNode {
            var nodes = new List<T>();
            while (parser.Matches(remainingStr).Success) {
                T node = ParseOne(parser)!;
                nodes.Add(node);
            }

            if (allowNull || nodes.Count > 0) return nodes;
            throw new FormatException(
                $"Expected at least one {typeof(T).Name} at: {remainingStr}"
            );
        }

        protected void consumeWhitespace() {
            Match whitespace = Regex.Match(remainingStr, @"^\s*");
            remainingStr = remainingStr.Substring(whitespace.Length);
        }

        private static void validateProgress<T>(T node, string input) where T : ASTNode {
            bool isSuffix = input.EndsWith(node.remainingStr, StringComparison.Ordinal);
            if (isSuffix && node.remainingStr.Length < input.Length) return;
            throw new InvalidOperationException(
                $"The {typeof(T).Name} parser did not consume an input prefix."
            );
        }
    }
}
