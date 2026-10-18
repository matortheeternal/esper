using esper.setup;
using esper.elements;

namespace esper.queries {
    public sealed class DeleteQuery : Query {
        public static Match Matches(string str) {
            return Regex.Match(str, @"^\s*DELETE\b\s*", RegexOptions.IgnoreCase);
        }

        public static DeleteQuery Parse(Match match, string str) {
            return new DeleteQuery(match, str);
        }

        private DeleteQuery(Match match, string str) : base(match, str) {
            ParseTargetAndSource();
            ParseWhereClause();
        }

        protected override QueryResult Execute(Session session) {
            Element[] selected = Select(session);
            foreach (Element element in selected)
                Validate(element);
            foreach (Element element in selected)
                if (!element.Remove())
                    throw new InvalidOperationException(
                        $"DELETE could not remove '{element.fullPath}'."
                    );
            return new QueryResult(selected);
        }

        private static void Validate(Element element) {
            if (element.def.required)
                throw new InvalidOperationException(
                    $"DELETE cannot remove required element '{element.fullPath}'."
                );
            if (element.container is not Container)
                throw new InvalidOperationException(
                    $"DELETE cannot remove '{element.fullPath}'."
                );
        }
    }
}
