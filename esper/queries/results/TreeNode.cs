#nullable enable

using esper.elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.queries {
    public abstract class TreeNode {
        private readonly Lazy<IReadOnlyList<TreeNode>> childNodes;

        public QueryResult result { get; }
        public TreeNode? parent { get; private set; }
        public IReadOnlyList<TreeNode> children { get { return childNodes.Value; } }
        public virtual IEnumerable<Element> elements {
            get { return children.SelectMany(child => child.elements); }
        }

        protected TreeNode(QueryResult result) {
            this.result = result;
            childNodes = new(() => []);
        }

        protected TreeNode(
            QueryResult result,
            Func<IReadOnlyList<TreeNode>> createChildren
        ) {
            this.result = result;
            childNodes = new(() => SetParents(createChildren()));
        }

        private IReadOnlyList<TreeNode> SetParents(
            IReadOnlyList<TreeNode> children
        ) {
            foreach (TreeNode child in children)
                child.parent = this;
            return children;
        }
    }

    internal sealed class RootNode : TreeNode {
        internal RootNode(
            QueryResult result,
            Func<IReadOnlyList<TreeNode>> createChildren
        ) : base(result, createChildren) { }
    }
}
