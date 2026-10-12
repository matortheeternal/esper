using esper.elements;
using esper.resolution;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbEFIXParamDecider: indexes the union by the
    // 'EFIX Param Info' value of the record's EFME subrecord.
    public class EFIXParamDecider : Decider {
        public override int Decide(Container container) {
            int? paramInfo = (int?)container?.GetData<int?>(
                @"..\EFME\EFIX Param Info");
            return paramInfo ?? 0;
        }
    }
}
