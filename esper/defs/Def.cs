using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Def {
        internal DefinitionManager manager;
        internal JObject src;
        internal Def parent;
        internal SessionOptions sessionOptions => manager.session.options;

        public Def(DefinitionManager manager, JObject src, Def parent) {
            this.manager = manager;
            this.src = src;
            if (parent != null) this.parent = parent;
        }
    }
}
