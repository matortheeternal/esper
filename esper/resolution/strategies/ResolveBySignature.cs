using esper.elements;
using esper.plugins;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveBySignature {
        private static readonly Regex signatureExpr = new Regex(@"^([^\\s]{4})($| - )");

        public static bool Valid(Element element) {
            return !(element is GroupRecord) && !(element is PluginFile);
        }

        public static MatchData Match(Element element, string pathPart) {
            if (!Valid(element)) return null;
            var c = ContainerMatch.From(element, pathPart, signatureExpr);
            if (c == null) return null;
            var sig = c.match.Groups[0].Value;
            if (!element.def.ContainsSignature(sig)) return null;
            return c;
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var sig = c.match.Groups[0].Value;
            foreach (Element element in c.container.elements)
                if (element.signature == sig) return element;
            return null;
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
