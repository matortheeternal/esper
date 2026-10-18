using esper.elements;
using esper.setup;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.queries {
    public abstract class QuerySource : ASTNode {
        protected QuerySource() { }

        protected QuerySource(Match match, string str) : base(match, str) { }

        public abstract IEnumerable<Element> Select(Session session);
    }
}
