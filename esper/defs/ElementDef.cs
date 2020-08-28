using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.defs {
    public class ElementDef : Def {
        public readonly string name;
        public readonly bool required;

        public virtual bool canContainFormIds => false;
        public virtual string signature => null;
        public virtual string displayName => name;
        public virtual int? size => 0;

        public virtual ReadOnlyCollection<ElementDef> childDefs => null;

        public ElementDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            name = src.Value<string>("name");
            required = src.Value<bool>("required");
        }

        public virtual bool ContainsSignature(string signature) {
            return false;
        }

        public virtual bool CanEnterWith(string signature) {
            return false;
        }

        public virtual List<string> GetSignatures(List<string> sigs = null) {
            throw new NotImplementedException();
        }

        public virtual void SubrecordFound(
            Container container, PluginFileSource source
        ) {
            throw new NotImplementedException();
        }

        public virtual Element NewElement(Container container) {
            throw new NotImplementedException();
        }

        public virtual Element ReadElement(
            Container container, PluginFileSource source, UInt16? size = null
        ) {
            throw new NotImplementedException();
        }

        public virtual Element PrepareElement(Container container) {
            throw new NotImplementedException();
        }

        public virtual bool IsSubrecord() {
            return signature != null;
        }

        public virtual bool HasSignature(string sig) {
            return signature == sig;
        }

        public virtual string GetSortKey(Element element) {
            throw new NotImplementedException();
        }

        internal virtual void WriteElement(
            Element element, PluginFileOutput output
        ) {
            throw new NotImplementedException();
        }

        internal virtual UInt32 GetSize(Element element) {
            if (element is Container container && container._internalElements != null) 
                return (UInt32) container._internalElements.Sum(e => e.size);
            return 0;
        }
    }
}
