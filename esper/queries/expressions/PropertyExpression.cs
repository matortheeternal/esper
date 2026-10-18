#nullable enable

using esper.elements;
using esper.resolution;
using System;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class PropertyExpression : Expression {
        public string name { get; }

        public static Match Matches(string str) {
            return Regex.Match(
                str,
                @"^(?:\[(?:""[^""]*""|'[^']*'|[^\]]*)\]|[A-Za-z_][A-Za-z0-9_]*)(?:\.(?:\[(?:""[^""]*""|'[^']*'|[^\]]*)\]|[A-Za-z_][A-Za-z0-9_]*))*\s*"
            );
        }

        public static PropertyExpression Parse(Match match, string str) {
            return new PropertyExpression(match, str);
        }

        public PropertyExpression(Match match, string str) : base(match, str) {
            name = match.Value.Trim();
        }

        public override object? Evaluate(Element element) {
            object? current = element;
            foreach (string part in name.Split('.')) {
                if (IsBracket(part)) {
                    string path = Unquote(part[1..^1]);
                    if (current is not Element currentElement)
                        throw UnknownProperty();
                    current = currentElement.ResolveElement(path);
                    if (current == null) throw UnknownProperty();
                    continue;
                }
                if (!(current is Element propertyElement)
                    || !ElementProperties.tryRead(part, propertyElement, out object? value))
                    throw UnknownProperty();
                current = value;
            }
            return current;
        }

        private InvalidOperationException UnknownProperty() {
            return new InvalidOperationException($"Unknown property '{name}'.");
        }

        private static bool IsBracket(string part) {
            return part.Length >= 2
                && part[0] == '['
                && part[^1] == ']';
        }

        private static string Unquote(string path) {
            if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
                return path[1..^1];
            if (path.Length >= 2 && path[0] == '\'' && path[^1] == '\'')
                return path[1..^1];
            return path;
        }
    }
}
