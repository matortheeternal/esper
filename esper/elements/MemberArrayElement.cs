using esper.defs;

namespace esper.elements {
    public class MemberArrayElement : Container {
        public MemberArrayDef maDef { get => def as MemberArrayDef;  } 

        public MemberArrayElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            maDef.memberDef.InitElement(this);
        }
    }
}
