using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbOMODDataPropertyValue1Decider: indexes the union by
    // the sibling 'Value Type' element, consulting the 'Property' name when
    // the value type is 5.
    public class OMODDataPropertyValue1Decider : Decider {
        public override int Decide(Container container) {
            int? valueType = (int?)container?.GetData<int?>("Value Type");
            if (valueType == null) return 0;
            switch (valueType) {
                case 0: return 1;
                case 1: return 2;
                case 2: return 3;
                case 4:
                case 6: return 4;
                case 5: {
                    var property = container.GetValue("Property");
                    if (property == "SoundLevel") return 6;
                    if (property == "StaggerValue") return 7;
                    if (property == "HitBehaviour") return 8;
                    return 5;
                }
                default: return 0;
            }
        }
    }
}
