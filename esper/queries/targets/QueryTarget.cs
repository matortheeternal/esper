using esper.elements;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.queries {
    public abstract class QueryTarget : ASTNode {
        protected QueryTarget(Match match, string str) : base(match, str) { }

        public abstract IEnumerable<Element> Select(IEnumerable<Element> source);

        protected static IEnumerable<Element> descendants(Element element) {
            if (element is not Container container) yield break;
            foreach (Element child in container.elements) {
                yield return child;
                foreach (Element descendant in descendants(child))
                    yield return descendant;
            }
        }
    }
}
