using esper.elements;

namespace esper.resolution {
    public class FormIdMatch : ElementMatch<Container> {
        public MainRecord rec;

        public FormIdMatch(Container element, Match match)
            : base(element, match) {
            target = element;
        }

        public new static FormIdMatch From(
            Element e, string pathPart, Regex expr
        ) {
            Match m = expr.Match(pathPart);
            if (m.Success && e is Container container)
                return new FormIdMatch(container, m);
            return null;
        }

    }
}
