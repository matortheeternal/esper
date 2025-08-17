using esper.setup;

namespace esper.defs {    public class Def {
        internal DefinitionManager manager;

        internal SessionOptions sessionOptions => manager.session.options;
        public virtual string description => throw new NotImplementedException();
        public virtual XEDefType defType => throw new NotImplementedException();

        public Def(DefinitionManager manager, JObject src) {
            this.manager = manager;
        }

        public Def(Def other) {
            manager = other.manager;
        }
    }
}
