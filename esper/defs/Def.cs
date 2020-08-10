using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Def {
        internal DefinitionManager manager;
        internal Def parent;
        internal SessionOptions sessionOptions => manager.session.options;

        public Def(DefinitionManager manager, JObject src, Def parent) {
            this.manager = manager;
            if (parent != null) this.parent = parent;
        }
    }
}
