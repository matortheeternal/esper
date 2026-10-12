using esper.elements;
using esper.resolution;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbMGEFFAssocItemDecider (FO3/FNV variant): indexes
    // the union by the sibling 'Archtype' value. Defaults to 1.
    public class MGEFFAssocItemDecider : Decider {
        public override int Decide(Container container) {
            int? archtype = (int?)container?.GetData<int?>("Archtype");
            if (archtype == null) return 1;
            switch (archtype) {
                case 1: return 2;
                case 18: return 3;
                case 19: return 4;
                default: return 0;
            }
        }
    }
}
