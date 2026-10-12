using esper.elements;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbXLOCFillerDecider: the XLOC struct carries a
    // 4-byte filler when its full subrecord data size is 16.
    public class XLOCFillerDecider : Decider {
        public override int Decide(Container container) {
            return Decide(container, null);
        }

        public override int Decide(Container container, UInt32? dataSize) {
            var size = DeciderHelpers.AncestorSubrecordSize(container);
            if (size == null) return 0;
            return size == 16 ? 1 : 0;
        }
    }
}
