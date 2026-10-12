using esper.elements;

namespace esper.defs {
    // Shared helpers for union deciders.
    public static class DeciderHelpers {
        // Returns the full data size of the nearest enclosing element that
        // was read from a sized source (e.g. a subrecord struct), walking up
        // from the union's container. Used by deciders that branch on the
        // size of the enclosing subrecord (XLOC, XSED, PKDT, IMGS DNAM).
        public static UInt32? AncestorSubrecordSize(Container container) {
            Element e = container;
            while (e != null) {
                if (e.sourceDataSize.HasValue) return e.sourceDataSize;
                e = e.container;
            }
            return null;
        }
    }
}
