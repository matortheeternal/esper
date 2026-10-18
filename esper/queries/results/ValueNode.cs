#nullable enable

namespace esper.queries {
    public sealed class ValueNode : TreeNode {
        public object? value { get; }

        internal ValueNode(QueryResult result, object? value) : base(result) {
            this.value = value;
        }
    }
}
