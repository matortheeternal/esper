using System;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class NodeParser<T> where T : ASTNode {
        public Func<string, Match> Matches { get; }
        public Func<Match, string, T> Parse { get; }

        public NodeParser(
            Func<string, Match> Matches,
            Func<Match, string, T> Parse
        ) {
            this.Matches = Matches;
            this.Parse = Parse;
        }
    }
}
