using esper.setup;
using esper.plugins;
using esper.resolution;
using esper.defs;
using System;

namespace esper.elements {
    public class Element : IResolution {
        public readonly ElementDef def;
        public ElementState state;
        public Container container { get; internal set; }
        public DefinitionManager manager => file.manager;
        public virtual string signature => def.signature;
        public virtual string name => def.name;
        public SessionOptions sessionOptions => manager.session.options;
        public Game game => manager.session.game;

        public MainRecord referencedRecord {
            get => throw new Exception("Element does not reference records.");
        }

        public virtual PluginFile file => container?.file;
        public virtual GroupRecord group => container?.group;
        public virtual MainRecord record => container?.record;
        public virtual Element subrecord {
            get {
                if (def == null) return null;
                return def.IsSubrecord() ? this : container?.subrecord;
            }
        }

        public Element(Container container = null, ElementDef def = null) {
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

        public virtual bool SupportsSignature(string sig) {
            return false;
        }
    }
}
