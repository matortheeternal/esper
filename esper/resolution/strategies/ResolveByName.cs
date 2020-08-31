using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public class ResolveByName : ResolutionStrategy {
        private static readonly Regex nameExpr = new Regex(@".+");
        public override bool canCreate => true;

        public override MatchData Match(Element element, string pathPart) {
            return ContainerMatch.From(element, pathPart, nameExpr);
        }

        public override Element Resolve(MatchData match) {
            var c = (ContainerMatch)match;
            foreach (var element in c.container.elements)
                if (element.name == c.match.Value) return element;
            return null;
        }

        public override Element Create(MatchData match) {
            ContainerMatch c = (ContainerMatch)match;
            var name = c.match.Value;
            return c.container.CreateElementByName(name);
        }
    }
}
