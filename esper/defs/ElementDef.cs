using esper.data.headers;
using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;
using esper.data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public virtual List<ElementDef> childDefs => null;

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
            conflictType = other.conflictType;
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
            Container container, RecordSource source
        ) {
            throw new NotImplementedException();
        }

        public virtual Element NewElement(Container container = null) {
            throw new NotImplementedException();
        }

        public virtual Element ReadElement(
            Container container, DataSource source, UInt32? size = null
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

        internal virtual GroupDef ImproviseGroupDef(IGroupHeader header) {
            var groupDef = manager.ImproviseGroupDef(header);
            childDefs.Add(groupDef);
            return groupDef;
        }

        internal virtual MainRecordDef ImproviseRecordDef(IRecordHeader header) {
            var recordDef = manager.GetRecordDef(header.signature) ?? 
                manager.ImproviseRecordDef(header);
            childDefs.Add(recordDef);
            return (MainRecordDef) recordDef;
        }

        internal virtual GroupDef GetGroupDef(IGroupHeader header) {
            foreach(var childDef in childDefs) {
                if (!(childDef is GroupDef groupDef)) continue;
                if (groupDef.groupType == header.groupType) 
                    return groupDef;
            }
            if (sessionOptions.improvise) return ImproviseGroupDef(header);
            throw new Exception("Valid group def not found.");
        }

        internal virtual MainRecordDef GetMainRecordDef(IRecordHeader header) {
            foreach (var childDef in childDefs) {
                if (!(childDef is MainRecordDef recordDef)) continue;
                if (recordDef.signature == header.signature)
                    return recordDef;
            }
            if (sessionOptions.improvise) return ImproviseRecordDef(header);
            throw new Exception("Valid record def not found.");
        }

        internal JArray ChildrenToJArray() {
            var children = new JArray();
            foreach (var childDef in childDefs) {
                if (this is TopGroupDef && childDef is MainRecordDef) continue;
                var isBase = !childDef.HasBaseDef();
                children.Add(childDef.ToJObject(isBase));
            }
            return children;
        }

        internal virtual JObject ToJObject(bool isBase = true) {
            if (!isBase) return new JObject { { "id", signature.ToString() } };
            var src = new JObject();
            if (signature != Signatures.None) src.Add("signature", signature.ToString());
            if (name != null) src.Add("name", name);
            if (required) src.Add("required", true);
            if (childDefs == null) return src;
            var children = ChildrenToJArray();
            if (children.Count > 0) src.Add("children", children);
            return src;
        }

        internal virtual void UpdateDef() {
            if (signature == Signatures.None) return;
            var sig = signature.ToString();
            manager.UpdateDef(sig, ToJObject());
        }

        internal virtual bool HasBaseDef() {
            if (signature == Signatures.None) return false;
            var sig = signature.ToString();
            return manager.HasDefEntry(sig);
        }
    }
}
