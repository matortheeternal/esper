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
        public PluginFile file { 
            get {
                if (this is PluginFile asFile) return asFile;
                return container.file;
            }
        }
        public ElementState state;
        public DefinitionManager manager => file.manager;
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
        public string name => def.GetName();

        public Element(Container container = null, Def def = null) {
            this.def = def;
            this.container = container;
            if (container == null) return;
            container.elements.Add(this);
        }

        public void SetState(ElementState state) {
            this.state |= state;
        }

        public void ClearState(ElementState state) {
            this.state ^= state;
        }
    }
}
