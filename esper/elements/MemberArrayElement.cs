using esper.defs;

namespace esper.elements {
    public class MemberArrayElement : Container {
        public MemberArrayDef maDef => def as MemberArrayDef;

        public MemberArrayElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            maDef.memberDef.InitElement(this);
        }

        public override bool SupportsSignature(string sig) {
            return maDef.memberDef.HasSignature(sig);
        }
    }
}
