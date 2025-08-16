using esper.elements;

namespace esper.resolution.strategies {
    [JSExport]
    public class ResolveChildGroup : ResolutionStrategy {
        private static readonly Regex expr = new Regex(@"^Child Group$");

        public override MatchData Match(Element element, string pathPart) {
             return ElementMatch<MainRecord>.From(element, pathPart, expr);
        }

        public override Element Resolve(MatchData match) {
            var m = (ElementMatch<MainRecord>)match;
            return m.target.childGroup;
        }

        public override Element Create(MatchData match) {
            var m = (ElementMatch<MainRecord>)match;
            return m.target.CreateChildGroup();
        }
    }
}
