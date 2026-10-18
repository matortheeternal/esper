using esper.elements;
using esper.setup;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class SelectionSource : QuerySource {
        public ulong selectionId { get; }

        public static Match Matches(string str) {
            return Regex.Match(
                str,
                @"^FROM\s+selection\s+(?<id>\d+)\b\s*",
                RegexOptions.IgnoreCase
            );
        }

        public static SelectionSource Parse(Match match, string str) {
            return new SelectionSource(match, str);
        }

        public SelectionSource(Match match, string str) : base(match, str) {
            selectionId = ulong.Parse(match.Groups["id"].Value);
        }

        public override IEnumerable<Element> Select(Session session) {
            return session.GetSelection(selectionId).elements;
        }
    }
}
