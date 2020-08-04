using esper.setup;
using esper.plugins;
using esper.resolution;
using esper.defs;
using System;
using System.Linq;

namespace esper.elements {
    public class Element : IResolution {
        public readonly Def def;
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

        public PluginFile file {
            get {
                if (this is PluginFile asFile) return asFile;
                return container?.file;
            }
        }

        public GroupRecord group {
            get {
                if (this is GroupRecord asGroup) return asGroup;
                return container?.group;
            }
        }

        public MainRecord record {
            get {
                if (this is MainRecord asRecord) return asRecord;
                return container?.record;
            }
        }

        public Element subrecord {
            get {
                if (def.IsSubrecord()) return this;
                return container?.subrecord;
            }
        }


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

        public virtual bool SupportsSignature(string sig) {
            return false;
        }
    }
}
