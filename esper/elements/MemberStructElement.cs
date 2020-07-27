using esper.defs;

namespace esper.elements {
    public class MemberStructElement : Container {
        public MemberStructDef msDef { get => def as MemberStructDef; }

        public MemberStructElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            msDef.InitChildElements(this);
        }
    }
}
