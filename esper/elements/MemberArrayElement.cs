using esper.defs;
using esper.data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace esper.elements {
    public class MemberArrayElement : Container {
        public MemberArrayDef maDef => def as MemberArrayDef;

        public override ReadOnlyCollection<Element> elements {
            get {
                if (!maDef.sorted) return base.elements;
                return internalElements
                    .OrderBy(e => e.sortKey)
                    .ToList().AsReadOnly();
            }
        }

        public MemberArrayElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            var e = maDef.memberDef.NewElement(this);
            e.Initialize();
        }

        public override bool SupportsSignature(Signature sig) {
            return maDef.memberDef.HasSignature(sig);
        }

        internal override AssignmentInfo GetAssignment(ElementDef childDef) {
            if (childDef != maDef.memberDef)
                throw new Exception($"Element {childDef.name} is not supported.");
            return new AssignmentInfo() {
                index = internalElements.Count
            };
        }

        internal override Element CreateDefault() {
            return maDef.memberDef.NewElement(this);
        }

        internal override bool RemoveElement(Element element) {
            if (internalElements.Count == 1) return Remove();
            if (!base.RemoveElement(element)) return false;
            maDef.ElementRemoved(this);
            return true;
        }

        internal override Element CopyInto(Container container, CopyOptions options) {
            var element = new MemberArrayElement(container, def);
            CopyChildrenInto(element, options);
            return element;
        }

        public override JToken ToJson() {
            return ToJsonArray();
        }
    }
}
