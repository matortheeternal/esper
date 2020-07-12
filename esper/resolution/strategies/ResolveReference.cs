using esper.elements;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveReference {
        private static readonly Regex refExpr = new Regex(@"^@(.+)");

        public static MatchData Match(Element element, string pathPart) {
            return ContainerMatch.From(element, pathPart, refExpr);
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            string pathPart = c.match.Groups[0].Value;
            Element element = c.container.GetElement(pathPart);
            return element.referencedRecord;
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
