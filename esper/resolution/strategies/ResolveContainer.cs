using esper.elements;

namespace esper.resolution.strategies {
    [JSExport]
    public class ResolveContainer : ResolutionStrategy {
        private static readonly Regex parentExpr = new Regex(@"^\.\.$");

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch<Element>.From(element, pathPart, parentExpr);
        }

        public override Element Resolve(MatchData match) {
            var e = (ElementMatch<Element>)match;
            if (e.target is GroupRecord group && group.hasRecordParent)
                return group.GetParentRecord();
            return e.target.container;
        }
    }
}
