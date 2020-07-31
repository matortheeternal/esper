using esper.elements;
using esper.plugins;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveGroupBySignature {
        private static readonly Regex signatureExpr = new Regex(@"^([^\\s]{4})($| - )");

        public static bool Valid(Element element) {
            return element is PluginFile;
        }

        public static MatchData Match(Element element, string pathPart) {
            if (!Valid(element)) return null;
            return ContainerMatch.From(element, pathPart, signatureExpr);
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var sig = c.match.Groups[0].Value;
            foreach (Element element in c.container.elements)
                if ((element is GroupRecord group) && 
                    (group.groupType == GroupType.Top) && 
                    (group.GetLabel().ToString() == sig)) return element;
            return null;
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
