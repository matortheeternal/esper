using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbDoorTriangleDoorTriangleDecider: the union holds a
    // door reference when the sibling DTUnknown value is non-zero.
    public class DoorTriangleDoorTriangleDecider : Decider {
        public override int Decide(Container container) {
            long? unknown = (long?)container?.GetData<long?>("DTUnknown");
            if (unknown == null) return 0;
            return unknown != 0 ? 1 : 0;
        }
    }
}
