using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution {
    public class ContainerMatch : MatchData {
        public Container container;

        public ContainerMatch(Container container, Match match)
            : base(match) {
            this.container = container;
        }

        public static ContainerMatch From(
            Element element, 
            string pathPart, 
            Regex expr
        ) {
            Match m = expr.Match(pathPart);
            if (m == null) return null;
            Container container = (Container)element;
            if (container == null) return null;
            return new ContainerMatch(container, m);
        }
    }
}
