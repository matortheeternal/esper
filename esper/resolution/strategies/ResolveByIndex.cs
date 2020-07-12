using esper.elements;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveByIndex {
        private static readonly Regex indexExpr = new Regex(@"^\[(\d+)\]$");

        public static MatchData Match(Element element, string pathPart) {
            return ContainerMatch.From(element, pathPart, indexExpr);
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            int index = int.Parse(c.match.Captures[0].Value);
            return c.container.elements[index];
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
