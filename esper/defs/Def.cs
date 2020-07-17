using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper {
    public class Def {
        internal DefinitionManager manager;
        internal JObject src;
        internal Def parent;

        public Def(DefinitionManager manager, JObject src, Def parent) {
            this.manager = manager;
            this.src = src;
            if (parent != null) this.parent = parent;
        }

        public virtual bool ContainsSignature(Signature signature) {
            return false;
        }

        public virtual bool HasPrimarySignature(Signature signature) {
            return false;
        }

        public virtual void SubrecordFound(Element element, Subrecord subrecord) {
            throw new NotImplementedException();
        }

        public virtual void DataFound(Element element, ReadOnlySpan<byte> ptr) {
            throw new NotImplementedException();
        }

        public virtual Element InitElement(Container container) {
            throw new NotImplementedException();
        }

        public virtual Element ReadElement(Container container, PluginFileSource source) {
            throw new NotImplementedException();
        }

        public virtual string GetName() {
            return src.Value<string>("name");
        }

        public virtual string GetSignature() {
            if (!src.ContainsKey("signature")) return null;
            return src.Value<string>("signature");
        }

        public virtual ushort GetSize() {
            if (!src.ContainsKey("size")) return 0;
            return src.Value<ushort>("size");
        }
    }
}
