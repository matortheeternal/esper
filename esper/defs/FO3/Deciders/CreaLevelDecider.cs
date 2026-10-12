using esper.elements;
using esper.resolution;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbCreaLevelDecider: if sibling Flags has 0x80 set,
    // the level is a curve table reference, else a plain level value.
    public class CreaLevelDecider : Decider {
        public override int Decide(Container container) {
            long? flags = (long?)container?.GetData<long?>("Flags");
            if (flags == null) return 0;
            return (flags & 0x80) != 0 ? 1 : 0;
        }
    }
}
