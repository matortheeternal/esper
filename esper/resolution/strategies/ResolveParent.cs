using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public class ResolveParent : ResolutionStrategy {
        private static readonly Regex refExpr = new Regex(@"^\^(.+)");

        public override MatchData Match(Element element, string pathPart) {
            return ElementMatch.From(element, pathPart, refExpr);
        }

        public override Element Resolve(MatchData match) {
            ElementMatch e = (ElementMatch)match;
            string type = e.match.Groups[0].Value;
            return type switch {
                "File" => e.element.file,
                "Group" => e.element.group,
                "Record" => e.element.record,
                "Subrecord" => e.element.subrecord,
                _ => null
            };
        }
    }
}
