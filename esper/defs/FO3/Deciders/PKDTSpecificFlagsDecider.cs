using esper.elements;
using esper.resolution;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbPKDTSpecificFlagsDecider: the 8-byte PKDT
    // subrecord has no type-specific flags; otherwise the union is indexed
    // by the sibling 'Type' value plus one.
    public class PKDTSpecificFlagsDecider : Decider {
        public override int Decide(Container container) {
            return Decide(container, null);
        }

        public override int Decide(Container container, UInt32? dataSize) {
            var size = DeciderHelpers.AncestorSubrecordSize(container);
            if (size == 8) return 0;
            int? type = (int?)container?.GetData<int?>("Type");
            if (type == null) return 0;
            return type.Value + 1;
        }
    }
}
