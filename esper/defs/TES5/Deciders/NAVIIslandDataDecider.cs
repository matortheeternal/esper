using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class NAVIIslandDataDecider : Decider {
        public override int Decide(Container container) {
            if (container == null) return 0;
            var e = container.GetParentElement(e => e.def.IsSubrecord());
            var isIsland = e?.GetElement("Is Island") as ValueElement;
            if (isIsland == null) return 0;
            return Convert.ToInt32(isIsland.data?.ToInt64() ?? 0);
        }
    }
}
