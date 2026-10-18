#nullable enable

using esper.elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace esper.queries {
    public sealed class GroupNode : TreeNode {
        private readonly IReadOnlyList<Element> groupElements;

        public Expression expression { get; }
        public object? key { get; }
        public IReadOnlyDictionary<string, object?> aggregateValues { get; }
        public override IEnumerable<Element> elements { get { return groupElements; } }

        internal GroupNode(
            QueryResult result,
            Expression expression,
            object? key,
            IReadOnlyList<Element> groupElements,
            IDictionary<string, object?> aggregateValues,
            Func<IReadOnlyList<TreeNode>> createChildren
        ) : base(result, createChildren) {
            this.expression = expression;
            this.key = key;
            this.groupElements = groupElements;
            this.aggregateValues = new ReadOnlyDictionary<string, object?>(
                aggregateValues
            );
        }
    }
}
