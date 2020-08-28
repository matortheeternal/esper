using esper.defs;
using esper.elements;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveBySignature {
        private static readonly Regex signatureExpr = new Regex(@"^([^\s]{4})($| - )");

        public static MatchData Match(Element element, string pathPart) {
            var c = ContainerMatch.From(element, pathPart, signatureExpr);
            if (c == null) return null;
            var sig = c.match.Groups[1].Value;
            return element.SupportsSignature(sig) ? c : null;
        }

        public static bool GroupMatches(Element element, string sig) {
            return element is GroupRecord group &&
                group.groupType == GroupType.Top &&
                group.GetLabel().ToString() == sig;
        }

        public static Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var sig = c.match.Groups[1].Value;
            foreach (Element element in c.container.elements) {
                if (GroupMatches(element, sig)) return element;
                if (element.signature == sig) return element;
            }
            return null;
        }

        public static ElementDef ResolveDef(Container container, string sig) {
            return container.def.childDefs.FirstOrDefault(def => {
                return def.signature == sig;
            });
        }

        public static Element Create(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var sig = c.match.Groups[1].Value;
            var targetDef = ResolveDef(c.container, sig);
            if (targetDef == null) 
                throw new Exception($"Failed to resolve def with signature: {sig}");
            var element = targetDef.NewElement(c.container);
            element.Initialize();
            return element;
        }
    }
}
