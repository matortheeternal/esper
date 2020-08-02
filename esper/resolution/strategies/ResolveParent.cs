using esper.elements;
using System;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveParent {
        private static readonly Regex parentExpr = new Regex(@"^\.\.$");

        public static MatchData Match(Element element, string pathPart) {
            return ElementMatch.From(element, pathPart, parentExpr);
        }

        public static Element Resolve(MatchData match) {
            ElementMatch e = (ElementMatch)match;
            if (e.element is GroupRecord group && group.isChildGroup)
                return group.GetParentRecord();
            return e.element.container;
        }

        public static Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
