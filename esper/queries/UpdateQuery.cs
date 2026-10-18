using esper.elements;
using System.Linq;
using System.Text.RegularExpressions;
using esper.setup;

namespace esper.queries {
    public sealed class UpdateQuery : Query {
        private static readonly NodeParser<SetClause> setParser =
            new(SetClause.Matches, SetClause.Parse);

        public SetClause setClause { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^\s*UPDATE\b\s*", RegexOptions.IgnoreCase);
        }

        public static UpdateQuery Parse(Match match, string str) {
            return new UpdateQuery(match, str);
        }

        private UpdateQuery(Match match, string str) : base(match, str) {
            ParseTargetAndSource();
            setClause = ParseOne(setParser)!;
            ParseWhereClause();
        }

        protected override QueryResult Execute(Session session) {
            Element[] selected = Select(session);
            string[] values = selected.Select(setClause.Evaluate).ToArray();
            foreach (Element element in selected)
                setClause.Validate(element);
            for (int i = 0; i < selected.Length; i++)
                setClause.Apply(selected[i], values[i]);
            return new QueryResult(selected);
        }
    }
}
