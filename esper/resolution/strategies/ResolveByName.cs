using esper.elements;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveByName {
        private static readonly Regex nameExpr = new Regex(@".+");

        public static MatchData Match(Element element, string pathPart) {
            return ContainerMatch.From(element, pathPart, nameExpr);
        }

        public static Element Resolve(MatchData match) {
            var c = (ContainerMatch)match;
            foreach (var element in c.container.elements)
                if (element.name == c.match.Value) return element;
            return null;
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
