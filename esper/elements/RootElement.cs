using esper.setup;

namespace esper.elements {
    public class RootElement : Container {
        private readonly Session session;

        public override DefinitionManager manager => session.definitionManager;
        public override string name => "Root";
        public override string displayName => name;

        public RootElement(Session session) : base(null, null) {
            this.session = session;
        }
    }
}
