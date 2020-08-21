using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class CTDAReferenceDecider : Decider {
        public override int Decide(Container container) {
            if (container?.GetData("Run On") == 2) return 1;
            return 0;
        }
    }
}
