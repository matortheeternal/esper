using esper.defs;

namespace esper.elements {
    public class UnionValueElement : ValueElement {
        public override string name => unionDef.name;
        public UnionDef unionDef => def.parent as UnionDef;

        public UnionValueElement(Container container, Def def, bool skipInit = false)
            : base(container, def, skipInit) {}
    }
}
