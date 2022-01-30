using esper.data.headers;
using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using esper.data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.defs {
    public class ElementDef : Def {
        public readonly bool required;
        public ConflictType conflictType { get; }
        // TODO: make a property for getConflictType callback
        public bool dynamicConflictType { get; }

        public virtual SmashType smashType => SmashType.stUnknown;
        public virtual bool canContainFormIds => false;
        public virtual Signature signature => Signatures.None;
        public virtual string name { get; }
        public virtual string displayName => name;
        public virtual int? size => 0;

        public virtual ReadOnlyCollection<ElementDef> childDefs => null;

        public ElementDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            name = src.Value<string>("name");
            required = src.Value<bool>("required");
            conflictType = JsonHelpers.ParseConflictType(src);
            dynamicConflictType = src.ContainsKey("getConflictType");
        }

        public ElementDef(ElementDef other) : base(other) {
            name = other.name;
            required = other.required;
        }

        public virtual bool ContainsSignature(Signature signature) {
            return false;
        }

        public virtual bool CanEnterWith(Signature signature) {
            return false;
        }

        public virtual HashSet<Signature> GetSignatures(HashSet<Signature> sigs = null) {
            throw new NotImplementedException();
        }

        public virtual void SubrecordFound(
            Container container, PluginFileSource source
        ) {
            throw new NotImplementedException();
        }

        public virtual Element NewElement(Container container = null) {
            throw new NotImplementedException();
        }

        public virtual Element ReadElement(
            Container container, PluginFileSource source, UInt32? size = null
        ) {
            throw new NotImplementedException();
        }

        internal bool ChildDefSupported(ElementDef def) {
            if (childDefs == null)
                throw new Exception($"{name} does not support child elements.");
            return childDefs.Contains(def);
        }

        public virtual Element PrepareElement(Container container) {
            throw new NotImplementedException();
        }

        public virtual bool NameMatches(string name) {
            return this.name == name;
        }

        public virtual bool IsSubrecord() {
            return signature != Signatures.None;
        }

        public virtual bool HasSignature(Signature sig) {
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

        internal virtual GroupDef GetGroupDef(IGroupHeader header) {
            foreach(var childDef in childDefs) {
                if (!(childDef is GroupDef groupDef)) continue;
                if (groupDef.groupType == header.groupType) 
                    return groupDef;
            }
            throw new Exception("Valid group def not found.");
        }
    }
}
