using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using esper.data;
using esper.defs;

namespace esper.elements {
    public class Container : Element {
        internal List<Element> _internalElements;
        internal MembersDef mdef => (MembersDef)def;

        internal List<Element> internalElements {
            get {
                if (_internalElements == null) 
                    _internalElements = new List<Element>();
                return _internalElements;
            }
        }

        public virtual ReadOnlyCollection<Element> elements {
            get => internalElements.AsReadOnly();
        }

        public int count => elements.Count;

        public Container(Container container = null, ElementDef def = null) 
            : base(container, def) {}

        public Element FindElementForDef(ElementDef def) {
            for (int i = elements.Count - 1; i >= 0; i--) {
                var element = elements[i];
                if (element.def == def) return element;
            }
            return null;
        }

        internal override void ElementsReady() {
            if (_internalElements == null) return;
            _internalElements.ForEach(e => e.ElementsReady());
        }

        internal virtual void ForEachElement(Func<Element, bool> callback) {
            internalElements.ForEach(element => {
                bool stepInto = callback(element);
                if (stepInto && element is Container container)
                    container.ForEachElement(callback);
            });
        }

        public virtual bool DefAssigned(ElementDef elementDef) {
            return false;
        }

        internal Element AssignDef(ElementDef def, bool replace) {
            var info = GetAssignment(def);
            if (info.assigned && !replace)
                return internalElements[index]; 
            var element = def.NewElement();
            info.Assign(this, element, replace);
            element.container = this;
            element.MarkModified();
            return element;
        }

        internal virtual AssignmentInfo GetAssignment(ElementDef childDef) {
            throw new NotImplementedException();
        }

        internal virtual Element CreateElementByName(string name) {
            var targetDef = def.childDefs?.FirstOrDefault(def => {
                return def.NameMatches(name);
            });
            if (targetDef == null)
                throw new Exception($"Cannot create element with name: {name}");
            if (targetDef is GroupDef groupDef)
                return groupDef.CreateFromName(this, name);
            return AssignDef(targetDef, false);
        }

        internal virtual Element CreateElementBySignature(string sig) {
            var signature = Signature.FromString(sig);
            var targetDef = def.childDefs?.FirstOrDefault(def => {
                return def.signature == signature;
            });
            if (targetDef == null)
                throw new Exception($"Cannot create element with signature: {sig}");
            return AssignDef(targetDef, false);
        }

        internal virtual Element CreateDefault() {
            throw new NotImplementedException();
        }

        internal virtual bool RemoveElement(Element element) {
            internalElements.Remove(element);
            return true;
        }

        internal void CopyChildrenInto(Container container, CopyOptions options) {
            internalElements.ForEach(el => el.CopyInto(container, options));
        }

        internal override void BuildRef() {
            foreach (var element in internalElements)
                if (element.def.canContainFormIds)
                    element.BuildRef();
        }
    }
}
