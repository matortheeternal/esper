using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class COEDOwnerDecider : Decider {
        public override int Decide(Container container) {
            MainRecord owner = (MainRecord) container?.GetElement("@Owner");
            return owner?.signature switch {
                "NPC_" => 1,
                "FACT" => 2,
                _ => 0
            };
        }
    }
}
