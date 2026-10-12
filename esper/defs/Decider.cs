using esper.elements;

namespace esper.defs {
    //[JSExport]
    public class Decider {
        public virtual int Decide(Container container) {
            throw new NotImplementedException();
        }

        // Overload receiving the size of the data being read, when known.
        // Direct-member subrecord unions get the subrecord data size.
        // Inline unions generally get the *remaining* size (or null), so
        // deciders that need the full enclosing subrecord size should use
        // DeciderHelpers.AncestorSubrecordSize instead.
        public virtual int Decide(Container container, UInt32? dataSize) {
            return Decide(container);
        }
    }
}
