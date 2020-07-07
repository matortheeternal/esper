using esper.defs;

namespace esper.elements {
    public class Element {
        readonly Def def;
        readonly Container container;
        readonly PluginFile file;

        public Element(Container container = null, Def def = null) {
            this.def = def;
            this.container = container;
            if (container == null) return;
            file = container.file;
        }

        /*public string getSignature() {
            MaybeSubrecordDef d = (MaybeSubrecordDef)def;
            
        }*/

    }
}
