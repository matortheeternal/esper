using esper.defs;

namespace esper.elements {
    public class MemberUnionElement : Container {
        public MemberUnionDef unionDef => def as MemberUnionDef;

        public MemberUnionElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            unionDef.defaultDef.InitElement(this);
        }
    }
}
