using esper.elements;

namespace esper.resolution.strategies {
    public class ResolveByName : ResolutionStrategy {
        private static readonly Regex nameExpr = new Regex(@".+");

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch<Container>.From(element, pathPart, nameExpr);
        }

        public override Element Resolve(MatchData match) {
            var c = (ElementMatch<Container>)match;
            var name = c.match.Value;
            foreach (var element in c.target.elements)
                if (element is MainRecord rec) {
                    if (rec.editorId == name) return rec;
                } else if (element.name == name) {
                    return element;
                }
            return null;
        }

        public override Element Create(MatchData match) {
            var c = (ElementMatch<Container>)match;
            var name = c.match.Value;
            return c.target.CreateElementByName(name);
        }
    }
}
