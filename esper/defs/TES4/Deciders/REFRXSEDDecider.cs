using esper.elements;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbREFRXSEDDecider: the XSED struct is 4 bytes when
    // it holds a seed value, else the union is empty.
    public class REFRXSEDDecider : Decider {
        public override int Decide(Container container) {
            return Decide(container, null);
        }

        public override int Decide(Container container, UInt32? dataSize) {
            var size = DeciderHelpers.AncestorSubrecordSize(container);
            if (size == null) return 0;
            return size == 4 ? 1 : 0;
        }
    }
}
