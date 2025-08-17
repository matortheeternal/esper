using esper.elements;

namespace esper.resolution.strategies {
    public class ResolveByFullName : ResolutionStrategy {
        private static readonly Regex nameExpr = new Regex("^\"(.+)\"$");

        public override MatchData Match(Element element, string pathPart) {
            if (element is GroupRecord group && group.isTopGroup)
                return ElementMatch<Container>.From(element, pathPart, nameExpr);
            return null;
        }

        public override Element Resolve(MatchData match) {
            var c = (ElementMatch<Container>)match;
            var name = c.match.Groups[1].Value;
            foreach (var element in c.target.elements)
                if (element is MainRecord rec && rec.fullName == name)
                    return rec;
            return null;
        }

        public override Element Create(MatchData match) {
            var c = (ElementMatch<Container>)match;
            var group = (GroupRecord)c.target;
            var sig = group.signature.ToString();
            var rec = (MainRecord)group.CreateElementBySignature(sig);
            rec.fullName = c.match.Value;
            return rec;
        }
    }
}
