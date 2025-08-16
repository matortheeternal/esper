using esper.elements;
using esper.plugins;

namespace esper.resolution.strategies {
    [JSExport]
    public class ResolveReference : ResolutionStrategy {
        private static readonly Regex refExpr = new Regex(@"^@(.+)");

        public bool Valid(Element element) {
            return !(element is GroupRecord)
                && !(element is PluginFile);
        }

        public override MatchData Match(Element element, string pathPart) {
            if (!Valid(element)) return null;
            return ElementMatch<Container>.From(element, pathPart, refExpr);
        }

        public override Element Resolve(MatchData match) {
            var c = (ElementMatch<Container>)match;
            string path = c.match.Groups[1].Value;
            Element element = c.target.ResolveElement(path);
            return element?.referencedRecord;
        }
    }
}
