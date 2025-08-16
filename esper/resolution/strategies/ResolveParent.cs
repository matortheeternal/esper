using esper.elements;

namespace esper.resolution.strategies {
    [JSExport]
    public class ResolveParent : ResolutionStrategy {
        private static readonly Regex refExpr = new Regex(@"^\^(.+)");

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch<Element>.From(element, pathPart, refExpr);
        }

        public override Element Resolve(MatchData match) {
            var e = (ElementMatch<Element>)match;
            string type = e.match.Groups[0].Value;
            return type switch {
                "File" => e.target.file,
                "Group" => e.target.group,
                "Record" => e.target.record,
                "Subrecord" => e.target.subrecord,
                _ => null
            };
        }
    }
}
