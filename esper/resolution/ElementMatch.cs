using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution {
    public class ElementMatch : MatchData {
        public Element element;

        public ElementMatch(Element element, Match match)
            : base(match) {
            this.element = element;
        }

        public static ElementMatch From(
            Element element,
            string pathPart,
            Regex expr
        ) {
            Match m = expr.Match(pathPart);
            if (m == null) return null;
            return new ElementMatch(element, m);
        }
    }
}
