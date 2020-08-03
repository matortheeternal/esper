using esper.elements;
using esper.resolution;
using System;

namespace esper.defs.TES5 {
    public class NAVIIslandDataDecider : Decider {
        public override int Decide(Container container) {
            var e = container.GetParentElement(e => e.def.IsSubrecord());
            var isIsland = e?.GetElement("Is Island");
            if (isIsland == null) return 0;
            return Math.Max(isIsland.GetData(), 1);
        }
    }
}
