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
        internal int sortOrder;

        internal SessionOptions sessionOptions => manager.session.options;
        public virtual string signature => src.Value<string>("signature");
        public virtual string name => src.Value<string>("name");
        public virtual int? size => src.Value<int?>("size");
        public bool required => src.Value<bool>("required");

        public Def(DefinitionManager manager, JObject src, Def parent) {
            this.manager = manager;
            this.src = src;
            if (parent != null) this.parent = parent;
        }

        public virtual bool ContainsSignature(string signature) {
            return false;
        }

        public virtual bool HasPrimarySignature(string signature) {
            return false;
        }

        public virtual void SubrecordFound(
            Container container, PluginFileSource source, string sig, UInt16 size
        ) {
            throw new NotImplementedException();
        }

        public virtual Element InitElement(Container container) {
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

        public bool IsSubrecord() {
            return signature != null;
        }
    }
}
