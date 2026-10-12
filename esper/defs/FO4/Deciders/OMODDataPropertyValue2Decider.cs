using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbOMODDataPropertyValue2Decider: indexes the union by
    // the sibling 'Value Type' element.
    public class OMODDataPropertyValue2Decider : Decider {
        public override int Decide(Container container) {
            int? valueType = (int?)container?.GetData<int?>("Value Type");
            if (valueType == null) return 0;
            switch (valueType) {
                case 0:
                case 4: return 1;
                case 1:
                case 6: return 2;
                case 2: return 3;
                default: return 0;
            }
        }
    }
}
