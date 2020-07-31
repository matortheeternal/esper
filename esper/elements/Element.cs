using esper.setup;
using esper.plugins;
using System;
using esper.resolution;

namespace esper.elements {
    public class Element : IResolution {
        public readonly Def def;
        public Container container { get; internal set; }
        public PluginFile file { 
            get {
                if (this is PluginFile asFile) return asFile;
                return container?.file;
            }
        }
        public ElementState state;
        public DefinitionManager manager => file.manager;
        public virtual string signature => def.signature;
        public MainRecord referencedRecord {
            get {
                throw new Exception("Element does not reference records.");
            }
        }
        public virtual string name => def.name;
        public SessionOptions sessionOptions => manager.session.options;


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

        public bool HasSubrecord(string sig) {
            return signature == sig;
        }
    }
}
