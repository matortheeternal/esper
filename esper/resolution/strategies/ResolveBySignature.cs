using esper.data;
using esper.elements;

namespace esper.resolution.strategies {
    [JSExport]
    public class ResolveBySignature : ResolutionStrategy {
        private static readonly Regex signatureExpr = new Regex(@"^([^\s]{4})($| - )");

        public override MatchData Match(Element element, string pathPart) {
            var c = ElementMatch<Container>.From(element, pathPart, signatureExpr);
            if (c == null) return null;
            var sig = Signature.FromString(c.match.Groups[1].Value);
            return element.SupportsSignature(sig) ? c : null;
        }

        public bool GroupMatches(Element element, string sig) {
            return element is GroupRecord group &&
                group.groupType == 0 &&
                group.label.ToString() == sig;
        }

        public override Element Resolve(MatchData match) {
            var c = (ElementMatch<Container>)match;
            var sig = Signature.FromString(c.match.Groups[1].Value);
            foreach (Element element in c.target.elements) {
                if (element.signature == sig) return element;
            }
            return null;
        }

        public override Element Create(MatchData match) {
            var c = (ElementMatch<Container>)match;
            var sig = c.match.Groups[1].Value;
            return c.target.CreateElementBySignature(sig);
        }
    }
}
