using Newtonsoft.Json.Linq;

namespace esper {
    public class Def {
        DefinitionManager manager;
        JObject src;
        Def parent = null;

        public Def(DefinitionManager manager, JObject src, Def parent = null) {
            this.manager = manager;
            this.src = src;
            if (parent != null) this.parent = parent;
        }

        public bool ContainsSignature(Signature signature) {
            return false;
        }

        public bool HasPrimarySignature(Signature signature) {
            throw new System.Exception("Unimplemented");
        }

        public string getName() {
            return src.Value<string>("name");
        }
    }
}
