using esper.elements;
using esper.plugins;

namespace esper.resolution.strategies {
    public class ResolveFirst : ResolutionStrategy {
        private static readonly Regex firstExpr = new Regex(@"^\[([^|]+(?:\|[^|]+)+)\]$");

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch<Container>.From(element, pathPart, firstExpr);
        }

        public override Element Resolve(MatchData match) {
            var c = (ElementMatch<Container>)match;
            List<string> pathParts = c.match.Groups[1].Value.Split("|").ToList();
            foreach (string pathPart in pathParts) {
                var e = c.target.ResolveElement(pathPart);
                if (e != null) return e;
            }
            return null;
        }
    }
}
