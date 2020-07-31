using esper.defs;

namespace esper.elements {
    public class StructElement : Container {
        public StructDef structDef => def as StructDef;

        public StructElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            structDef.InitChildElements(this);
        }
    }
}
