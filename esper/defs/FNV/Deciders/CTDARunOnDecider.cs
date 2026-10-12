using esper.elements;
using esper.resolution;

namespace esper.defs.FNV {
    // Port of xEdit 4.0.4 wbCTDARunOnDecider (FNV only): the union holds a
    // reference when the condition function targets the subject's direction.
    public class CTDARunOnDecider : Decider {
        public override int Decide(Container container) {
            int? function = (int?)container?.GetData<int?>("Function");
            if (function == null) return 0;
            return function == 106 || function == 285 ? 1 : 0;
        }
    }
}
