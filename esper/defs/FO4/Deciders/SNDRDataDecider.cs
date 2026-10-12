using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbSNDRDataDecider: auto-weapon sounds (CNAM native
    // value 3977607907) store value data, others store a base descriptor.
    public class SNDRDataDecider : Decider {
        public override int Decide(Container container) {
            long? cnam = (long?)container?.record?.GetData<long?>("CNAM");
            if (cnam == null) return 0;
            return cnam == 3977607907 ? 1 : 0;
        }
    }
}
