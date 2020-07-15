using esper.defs;
using esper.parsing;

namespace esper.elements {
    public class StructElement : Container {
        public StructDef structDef { get => (StructDef)def; }

        public StructElement(Container container, Def def)
            : base(container, def) { }

        public static StructElement Read(
            Container container, Def def, PluginFileSource source
        ) {
            var element = new StructElement(container, def);
            element.structDef.Read(element, source);
            return element;
        }
    }
}
