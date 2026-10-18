using System.Text.RegularExpressions;

namespace esper.queries {
    public abstract class QueryClause : ASTNode {
        protected QueryClause() { }

        protected QueryClause(Match match, string str) : base(match, str) { }
    }
}
