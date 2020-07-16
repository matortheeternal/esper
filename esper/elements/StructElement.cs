using esper.defs;
using esper.parsing;

namespace esper.elements {
    public class StructElement : Container {
        public StructDef structDef { get => (StructDef)def; }

        public StructElement(Container container, Def def)
            : base(container, def) {
            structDef.InitChildElements(this);
        }

        public StructElement(Container container, Def def, PluginFileSource source)
            : base(container, def) {
            structDef.ReadChildElements(this, source);
        }
    }
}
