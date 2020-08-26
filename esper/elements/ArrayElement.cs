﻿using esper.defs;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.elements {
    public class ArrayElement : Container {
        public ArrayDef arrayDef => (ArrayDef) def;

        public override ReadOnlyCollection<Element> elements {
            get {
                if (!arrayDef.sorted) return base.elements;
                return internalElements
                    .OrderBy(e => e.sortKey)
                    .ToList().AsReadOnly();
            }
        }

        public ArrayElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            var e = arrayDef.elementDef.NewElement(this);
            e.Initialize();
        }
    }
}
