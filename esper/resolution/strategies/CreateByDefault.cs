using esper.elements;

namespace esper.resolution.strategies {
    public class CreateByDefault : ResolutionStrategy {
        private static readonly Regex expr = new Regex(@"^(?:\.|\[\+\])$");
        public override bool canResolve => false;

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch<Container>.From(element, pathPart, expr);
        }

        public override Element Create(MatchData match) {
            var c = (ElementMatch<Container>)match;
            return c.target.CreateDefault();
        }
    }
}
