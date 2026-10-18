using esper.elements;
using esper.setup;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class SessionSource : QuerySource {
        public static Match Matches(string str) {
            return Regex.Match(
                str,
                @"^FROM\s+session\b\s*",
                RegexOptions.IgnoreCase
            );
        }

        public static SessionSource Parse(Match match, string str) {
            return new SessionSource(match, str);
        }

        internal static SessionSource defaultFor(string str) {
            return new SessionSource(str);
        }

        public SessionSource(Match match, string str) : base(match, str) { }

        private SessionSource(string str) {
            remainingStr = str;
        }

        public override IEnumerable<Element> Select(Session session) {
            return session.pluginManager.plugins
                .SelectMany(plugin => plugin.records)
                .Cast<Element>();
        }
    }
}
