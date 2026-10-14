using esper.defs;
using esper.elements;

namespace esper.setup {
    // Placeholder returned by GetDecider for unimplemented decider keys.
    // Lives in esper.setup (not esper.defs) so class scanning never
    // auto-registers it as a real decider. Only throws if data actually
    // routes through it.
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
