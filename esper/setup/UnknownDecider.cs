using esper.defs;
using esper.elements;

namespace esper.setup {
    public class UnknownDecider : Decider {
        private readonly string key;

        public UnknownDecider(string key) {
            this.key = key;
        }

        public override int Decide(Container container) {
            throw new Exception($"Decider not implemented: {key}");
        }
    }
}
