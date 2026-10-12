using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class CTDAReferenceDecider : Decider {
        public override int Decide(Container container) {
            var runOn = container?.GetElement("Run On") as ValueElement;
            if (runOn != null && Convert.ToInt32(runOn.data) == 2) return 1;
            return 0;
        }
    }
}
