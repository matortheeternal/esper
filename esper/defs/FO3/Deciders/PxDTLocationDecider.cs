using esper.elements;
using esper.resolution;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbPxDTLocationDecider: indexes the union by the
    // native value of the sibling 'Type' element.
    public class PxDTLocationDecider : Decider {
        public override int Decide(Container container) {
            int? type = (int?)container?.GetData<int?>("Type");
            return type ?? 0;
        }
    }
}
