using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class PerkDATADecider : Decider {
        public override int Decide(Container container) {
            var type = container.GetData(@"PRKE\Type");
            return type ?? 0;
        }
    }
}
