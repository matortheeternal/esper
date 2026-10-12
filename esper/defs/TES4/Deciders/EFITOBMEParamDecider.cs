using esper.elements;
using esper.resolution;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbEFITOBMEParamDecider: indexes the union by the
    // 'EFIT Param Info' value of the record's EFME subrecord.
    public class EFITOBMEParamDecider : Decider {
        public override int Decide(Container container) {
            int? paramInfo = (int?)container?.GetData<int?>(
                @"..\EFME\EFIT Param Info");
            return paramInfo ?? 0;
        }
    }
}
