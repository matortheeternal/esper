using esper.elements;

namespace esper.resolution {
    public class ElementMatch<T> : MatchData {
        public T target;

        public ElementMatch(T element, Match match)
            : base(match) {
            target = element;
        }

        public static ElementMatch<T> From(
            Element e, string pathPart, Regex expr
        ) {
            Match m = expr.Match(pathPart);
            if (m.Success && e is T element)
                return new ElementMatch<T>(element, m);
            return null;
        }
    }
}
