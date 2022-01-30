using esper.elements;
using esper.resolution;
using esper.data;
using System;

namespace esper.defs.TES5 {
    public class NVNMParentDecider : Decider {
        public override int Decide(Container container) {
            var rec = container?.group?.GetParentRecord();
            if (rec == null) return 0; // TODO?
            if (rec.signature != Signatures.CELL) 
                throw new Exception("Parent of a NVNM is not a CELL");
            var d = rec.GetElement("DATA");
            if (d == null) return 0;
            return (d.GetData() & 1) != 0 ? 1 : 0;
        }
    }
}
