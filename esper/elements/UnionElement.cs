using esper.defs;

namespace esper.elements {
    public class UnionElement : Container {
        public UnionDef unionDef => def as UnionDef;

        public UnionElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            var resolvedDef = unionDef.ResolveDef(container);
            resolvedDef.InitElement(this);
        }
    }
}
