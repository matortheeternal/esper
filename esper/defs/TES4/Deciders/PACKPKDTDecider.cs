using esper.elements;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbPACKPKDTDecider: the PKDT subrecord union holds
    // idle data when its data size is 4, else the full struct.
    public class PACKPKDTDecider : Decider {
        public override int Decide(Container container) {
            return Decide(container, null);
        }

        public override int Decide(Container container, UInt32? dataSize) {
            if (dataSize == null) return 1;
            return dataSize == 4 ? 0 : 1;
        }
    }
}
