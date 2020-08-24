using esper.setup;
using esper.plugins;
using esper.resolution;
using esper.defs;
using System;

namespace esper.elements {
    public class Element : IResolution {
        public readonly ElementDef def;
        public ElementState state;

        public virtual Container container { get; internal set; }
        public virtual DefinitionManager manager => file?.manager;
        public string sortKey => def?.GetSortKey(this);
        public virtual string name => def?.name;
        public virtual string signature => def?.signature;
        public virtual string displayName => def?.displayName;
        public virtual UInt16 size => 0;
        public SessionOptions sessionOptions => manager?.session.options;
        public Game game => manager?.session.game;
        public int index => container._elements.IndexOf(this);

        public virtual MainRecord referencedRecord {
            get => throw new Exception("Element does not reference records.");
            set => throw new Exception("Element does not reference records.");
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

        public virtual string pathKey {
            get {
                if (container is ArrayElement || container is MemberArrayElement)
                    return $"[{index}]";
                return signature ?? name;
            }
        }

        public virtual string path {
            get {
                if (container is MainRecord) return pathKey;
                var parentPath = container?.path;
                return parentPath == null
                    ? pathKey
                    : $"{parentPath}\\{pathKey}";
            }
        }

        public virtual string fullPath {
            get {
                var parentPath = container?.fullPath;
                return parentPath == null
                    ? pathKey
                    : $"{parentPath}\\{pathKey}";
            }
        }

        public Element(Container container = null, ElementDef def = null) {
            this.def = def;
            this.container = container;
            if (container == null) return;
            container.elements.Add(this);
        }

        public virtual void Initialize() {}

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

        internal virtual void ElementsReady() { }

        internal virtual void WriteTo(PluginFileOutput output) {
            def.WriteElement(this, output);
        }
    }
}
