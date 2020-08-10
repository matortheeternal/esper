using esper.defs;

namespace esper.elements {
    public class UnionValueElement : ValueElement {
        public UnionDef unionDef;

        public override string name => unionDef.name;

        public UnionValueElement(
            Container container, ElementDef def, UnionDef unionDef, 
            bool skipInit = false
        ) : base(container, def, skipInit) {
            this.unionDef = unionDef;
        }
    }
}
