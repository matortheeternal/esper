using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public class ResolveByIndex : ResolutionStrategy {
        private static readonly Regex indexExpr = new Regex(@"^\[(\d+)\]$");

        public override MatchData Match(Element element, string pathPart) {
            return ContainerMatch.From(element, pathPart, indexExpr);
        }

        public override Element Resolve(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            int index = int.Parse(c.match.Groups[1].Value);
            return c.container.elements[index];
        }
    }
}
