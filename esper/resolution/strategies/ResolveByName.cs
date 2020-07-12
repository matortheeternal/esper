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
            throw new NotImplementedException();
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
