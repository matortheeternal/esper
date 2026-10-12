using esper.elements;
using esper.resolution;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbNOTESNAMDecide: when the record's DATA subrecord
    // is 3 (Voice), SNAM holds an NPC/Actor reference, else a sound.
    public class NOTESNAMDecide : Decider {
        public override int Decide(Container container) {
            int? data = (int?)container?.record?.GetData<int?>("DATA");
            return data == 3 ? 1 : 0;
        }
    }
}
