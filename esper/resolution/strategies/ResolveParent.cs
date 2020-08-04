using esper.elements;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public static class ResolveParent {
        private static readonly Regex refExpr = new Regex(@"^\^(.+)");

        public static MatchData Match(Element element, string pathPart) {
            return ElementMatch.From(element, pathPart, refExpr);
        }

        public static Element Resolve(MatchData match) {
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

        public static Element Create(MatchData match) {
            return Resolve(match);
        }
    }
}
