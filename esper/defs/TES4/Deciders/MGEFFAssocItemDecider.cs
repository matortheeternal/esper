using esper.elements;
using esper.resolution;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbMGEFFAssocItemDecider (TES4 variant, Flags bits):
    // s[17] -> 1, s[18] -> 2, s[19] -> 3, s[25] -> 4, else 0.
    // xEdit flags are 1-indexed; esper bit i is xEdit s[i + 1].
    public class MGEFFAssocItemDecider : Decider {
        public override int Decide(Container container) {
            long? flags = (long?)container?.GetData<long?>("Flags");
            if (flags == null) return 0;
            if ((flags & (1L << 16)) != 0) return 1;
            if ((flags & (1L << 17)) != 0) return 2;
            if ((flags & (1L << 18)) != 0) return 3;
            if ((flags & (1L << 24)) != 0) return 4;
            return 0;
        }
    }
}
