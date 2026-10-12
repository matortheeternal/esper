using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbNOTEDataDecider: indexes the SNAM union by the
    // record's DNAM value (0 -> Sound, 1 -> Scene, 3 -> Terminal).
    public class NOTEDataDecider : Decider {
        public override int Decide(Container container) {
            int? data = (int?)container?.record?.GetData<int?>("DNAM");
            if (data == null) return 0;
            switch (data) {
                case 0: return 1;
                case 1: return 2;
                case 3: return 3;
                default: return 0;
            }
        }
    }
}
