using esper.defs;
using esper.data;

namespace esper.elements {
    public class MemberUnionElement : Container {
        public MemberUnionDef unionDef => def as MemberUnionDef;

        public MemberUnionElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {}

        public override void Initialize() {
            var e = unionDef.defaultDef.NewElement(this);
            e.Initialize();
        }

        public override bool SupportsSignature(Signature sig) {
            return unionDef.memberDefs.Any(d => d.HasSignature(sig));
        }

        internal override AssignmentInfo GetAssignment(ElementDef childDef) {
            throw new Exception("Cannot create MemberUnionElement child");
        }

        internal override bool RemoveElement(Element element) {
            return Remove();
        }

        internal override Element CopyInto(Container container, CopyOptions options) {
            var element = new MemberUnionElement(container, def);
            CopyChildrenInto(element, options);
            return element;
        }
    }
}
