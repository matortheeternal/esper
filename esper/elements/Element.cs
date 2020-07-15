using esper.defs;
using esper.setup;
using esper.plugins;
using esper.parsing;
using System;
using esper.resolution;

namespace esper.elements {
    public class Element : IResolution {
        public readonly Def def;
        public readonly Container container;
        public readonly PluginFile file;
        public ElementState state;
        public DefinitionManager manager {
            get {
                return file.manager;
            }
        }
        public Signature signature {
            get {
                MaybeSubrecordDef d = (MaybeSubrecordDef)def;
                if (d == null || !d.IsSubrecord()) return null;
                return d.signature;
            }
        }
        public MainRecord referencedRecord {
            get {
                throw new Exception("Element does not reference records.");
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
    }
}
