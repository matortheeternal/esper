using esper.defs;
using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public class ResolveBySignature : ResolutionStrategy {
        private static readonly Regex signatureExpr = new Regex(@"^([^\s]{4})($| - )");

        public override MatchData Match(Element element, string pathPart) {
            var c = ContainerMatch.From(element, pathPart, signatureExpr);
            if (c == null) return null;
            var sig = c.match.Groups[1].Value;
            return element.SupportsSignature(sig) ? c : null;
        }

        public bool GroupMatches(Element element, string sig) {
            return element is GroupRecord group &&
                group.groupType == (int)GroupType.Top &&
                group.label.ToString() == sig;
        }

        public override Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var sig = c.match.Groups[1].Value;
            foreach (Element element in c.container.elements) {
                if (element.signature == sig) return element;
            }
            return null;
        }

        public override Element Create(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var sig = c.match.Groups[1].Value;
            return c.container.CreateElementBySignature(sig);
        }
    }
}
