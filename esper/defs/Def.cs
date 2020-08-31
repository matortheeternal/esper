using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Def {
        internal DefinitionManager manager;
        internal SessionOptions sessionOptions => manager.session.options;

        public Def(DefinitionManager manager, JObject src) {
            this.manager = manager;
        }

        public Def(Def other) {
            manager = other.manager;
        }
    }
}
