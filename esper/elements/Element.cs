using esper.defs;
using esper.setup;

namespace esper.elements {
    public class Element {
        public readonly Def def;
        public readonly Container container;
        public readonly PluginFile file;
        public ElementState state;
        public DefinitionManager manager {
            get {
                return file.manager;
            }
        }

        public Element(Container container = null, Def def = null) {
            this.def = def;
            this.container = container;
            if (container == null) return;
            file = container.file;
        }

        public void SetState(ElementState state) {
            this.state |= state;
        }

        public void ClearState(ElementState state) {
            this.state ^= state;
        }

        public string GetSignature() {
            MaybeSubrecordDef d = (MaybeSubrecordDef)def;
            if (d == null || !d.IsSubrecord()) return null;
            return d.signature.ToString();
            
        }

    }
}
