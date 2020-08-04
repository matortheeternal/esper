using esper.elements;
using esper.plugins;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveReference {
        private static readonly Regex refExpr = new Regex(@"^@(.+)");

        public static bool Valid(Element element) {
            return !(element is GroupRecord)
                && !(element is PluginFile);
        }

        public static MatchData Match(Element element, string pathPart) {
            if (!Valid(element)) return null;
            return ContainerMatch.From(element, pathPart, refExpr);
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            string path = c.match.Groups[1].Value;
            Element element = c.container.ResolveElement(path);
            return element?.referencedRecord;
        }

        public static Element Create(MatchData match) {
            return Resolve(match);
        }
    }
}
