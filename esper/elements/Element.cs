using esper.setup;
using esper.plugins;
using esper.resolution;
using esper.defs;
using System;
using System.Collections.Generic;
using esper.data;

namespace esper.elements {
    public class Element : IResolution {
        public readonly ElementDef def;
        public ElementState state;

        public virtual Container container { get; internal set; }
        public virtual DefinitionManager manager => file.manager;
        public virtual string sortKey => def.GetSortKey(this);
        public virtual string name => def.name;
        public virtual Signature signature => def.signature;
        public virtual string displayName => def.displayName;
        public virtual UInt32 size => def.GetSize(this);
        public SessionOptions sessionOptions => manager.session.options;
        public Game game => manager.session.game;
        public virtual int index => container.elements.IndexOf(this);

        public virtual MainRecord referencedRecord {
            get => throw new Exception("Element does not reference records.");
            set => throw new Exception("Element does not reference records.");
        }

        public virtual PluginFile file => container.file;
        public virtual GroupRecord group => container.group;
        public virtual MainRecord record => container.record;
        public virtual Element subrecord {
            get {
                return def.IsSubrecord() ? this : container.subrecord;
            }
        }

        public virtual string pathKey {
            get {
                if (container is ArrayElement || container is MemberArrayElement)
                    return $"[{index}]";
                return def.IsSubrecord() ? signature.ToString() : name;
            }
        }

        internal void MarkChildrenModified() {
            if (!(this is Container container)) return;
            container.internalElements.ForEach(e => {
                e.SetState(ElementState.Modified);
                e.MarkChildrenModified();
            });
        }

        internal void MarkModified() {
            SetState(ElementState.Modified);
            MarkChildrenModified();
            var parent = container;
            while (parent != null) {
                parent.SetState(ElementState.Modified);
                parent = parent.container;
            }
        }

        public virtual string path {
            get {
                if (container is MainRecord) return pathKey;
                var parentPath = container.path;
                return parentPath == null
                    ? pathKey
                    : $"{parentPath}\\{pathKey}";
            }
        }

        public virtual string fullPath {
            get {
                var parentPath = container.fullPath;
                return parentPath == null
                    ? pathKey
                    : $"{parentPath}\\{pathKey}";
            }
        }

        public Element(Container container = null, ElementDef def = null) {
            this.def = def;
            this.container = container;
            if (container == null) return;
            container.internalElements.Add(this);
        }

        public virtual void Initialize() {}

        public void SetState(ElementState state) {
            this.state |= state;
        }

        public void ClearState(ElementState state) {
            this.state ^= state;
        }

        public bool HasSubrecord(Signature sig) {
            return def.signature == sig;
        }

        public virtual bool SupportsSignature(Signature sig) {
            return false;
        }

        internal virtual void ElementsReady() { }

        internal virtual void WriteTo(PluginFileOutput output) {
            def.WriteElement(this, output);
        }

        public virtual bool Remove() {
            if (def.required) return false;
            return container.RemoveElement(this);
        }

        internal virtual Element ResolveIn(Container container) {
            throw new NotImplementedException();
        }

        internal virtual Element CopyInto(Container container, CopyOptions options) {
            throw new NotImplementedException();
        }

        internal virtual void BuildRef() {
            throw new NotImplementedException();
        }

        internal Container ForceContainer(
            Container source, Container target, CopyOptions options, ref bool creating
        ) {
            if (!creating) {
                var child = source.ResolveIn(target);
                creating = child == null;
                if (!creating) return (Container)child;
            }
            return (Container)source.CopyInto(target, options);
        }
        
        public virtual Container ForceContainers(Element element, CopyOptions options) {
            var parentContainers = new List<Container>();
            var targetContainer = element.container;
            while (targetContainer != null) {
                if (def == targetContainer.def) break;
                parentContainers.Add(targetContainer);
                targetContainer = targetContainer.container;
            }
            if (targetContainer == null) 
                throw new Exception($"{element.def.description} is not supported by {def.description}");
            bool creating = false;
            var ccOptions = options ^ CopyOptions.CopyChildGroups;
            parentContainers.ForEach(parentContainer => {
                targetContainer = ForceContainer(
                    parentContainer, targetContainer, ccOptions, ref creating
                );
            });
            return targetContainer;
        }

        internal Element AssignCopy(Element element, CopyOptions options) {
            var container = ForceContainers(element, options);
            return element.ResolveIn(container) ??
                   element.CopyInto(container, options);
        }

        public virtual Element CopyTo(Element target, CopyOptions options) {
            if (target is MainRecord) {
                if (def is MaybeSubrecordDef)
                    return target.AssignCopy(this, options);
                throw new Exception($"Cannot copy ${def.displayName} into Main Record");
            } else if (target.def is ArrayDef || target.def is MemberArrayDef) {
                if (def is MaybeSubrecordDef && target.def.ChildDefSupported(def))
                    return target.AssignCopy(this, options);
                throw new Exception($"Cannot copy ${def.displayName} into array element");
            }
            throw new Exception($"Cannot copy elements into ${target.def.displayName}");
        }
    }
}
