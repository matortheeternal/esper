using esper.elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.queries {
    public sealed class QuerySelection {
        public ulong id { get; }
        public IReadOnlyList<Element> elements { get; }

        internal QuerySelection(ulong id, IEnumerable<Element> elements) {
            this.id = id;
            this.elements = Array.AsReadOnly(elements.ToArray());
        }
    }
}
