using esper.elements;
using esper.parsing;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveBySignature {
        private static readonly Regex signatureExpr = new Regex(@"^([^\\s]{4})($| - )");

        public static MatchData Match(Element element, string pathPart) {
            return ContainerMatch.From(element, pathPart, signatureExpr);
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            Signature sig = Signature.FromString(c.match.Groups[0].Value);
            foreach (Element element in c.container.elements)
                if (element.signature == sig) return element;
            return null;
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
