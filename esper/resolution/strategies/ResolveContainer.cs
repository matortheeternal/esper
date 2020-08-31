using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public class ResolveContainer : ResolutionStrategy {
        private static readonly Regex parentExpr = new Regex(@"^\.\.$");

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch.From(element, pathPart, parentExpr);
        }

        public override Element Resolve(MatchData match) {
            ElementMatch e = (ElementMatch)match;
            if (e.element is GroupRecord group && group.hasRecordParent)
                return group.GetParentRecord();
            return e.element.container;
        }
    }
}
