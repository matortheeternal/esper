using esper.elements;
using esper.resolution;
using System;

namespace esper.defs.TES5 {
    public class NVNMParentDecider : Decider {
        public override int Decide(Container container) {
            // TODO: make this better
            GroupRecord group = (GroupRecord) container.GetParentElement(
                e => e is GroupRecord
            );
            if (group == null) return 0;
            var rec = group.GetParentRecord();
            if (rec == null) return 0; // TODO?
            if (rec.signature != "CELL") 
                throw new Exception("Parent of a NVNM is not a CELL");
            var d = rec.GetElement("DATA");
            if (d == null) return 0;
            return (d.GetData() & 1) != 0 ? 1 : 0;
        }
    }
}
