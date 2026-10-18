using esper.elements;
using System.Collections.Generic;

namespace esper.queries {
    public sealed class ElementNode : TreeNode {
        public Element element { get; }
        public override IEnumerable<Element> elements { get { return [element]; } }

        internal ElementNode(QueryResult result, Element element) : base(result) {
            this.element = element;
        }
    }
}
