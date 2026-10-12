using esper.elements;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbIMGSSkinDimmerDecider: the union holds a float
    // when the enclosing DNAM subrecord data size is in [132, 148].
    public class IMGSSkinDimmerDecider : Decider {
        public override int Decide(Container container) {
            return Decide(container, null);
        }

        public override int Decide(Container container, UInt32? dataSize) {
            var size = DeciderHelpers.AncestorSubrecordSize(container);
            if (size == null) return 0;
            return size >= 132 && size <= 148 ? 1 : 0;
        }
    }
}
