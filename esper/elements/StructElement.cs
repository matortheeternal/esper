using esper.defs;

namespace esper.elements {
    public class StructElement : Container {
        public StructDef structDef => def as StructDef;

        public StructElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            structDef.InitChildElements(this);
        }

        internal override void ElementsReady() {
            base.ElementsReady();
            structDef.RemapElements(this);
        }
    }
}
