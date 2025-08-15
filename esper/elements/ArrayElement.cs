using esper.defs;
using Newtonsoft.Json.Linq;
using System;
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

        internal override AssignmentInfo GetAssignment(ElementDef childDef) {
            if (childDef != arrayDef.elementDef)
                throw new Exception($"Element {childDef.name} is not supported.");
            return new AssignmentInfo() { 
                index = internalElements.Count 
            };
        }

        internal override Element CreateDefault() {
            return arrayDef.elementDef.NewElement(this);
        }

        internal override bool RemoveElement(Element element) {
            if (internalElements.Count == 1) return Remove();
            if (!base.RemoveElement(element)) return false;
            arrayDef.ElementRemoved(this);
            return true;
        }

        internal override Element CopyInto(Container container, CopyOptions options) {
            var element = new ArrayElement(container, def);
            CopyChildrenInto(element, options);
            return element;
        }

        public override JToken ToJson() {
            return ToJsonArray();
        }
    }
}
