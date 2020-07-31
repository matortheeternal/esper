using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution {
    public class ContainerMatch : MatchData {
        public readonly Container container;

        public ContainerMatch(Container container, Match match) : base(match) {
            this.container = container;
        }

        public static ContainerMatch From(
            Element element, string pathPart, Regex expr
        ) {
            Match m = expr.Match(pathPart);
            if (m.Success && element is Container container) 
                return new ContainerMatch(container, m);
            return null;
        }
    }
}
