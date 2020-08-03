using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class CTDACompValueDecider : Decider {
        public override int Decide(Container container) {
            if (container == null) return 0;
            var type = container.GetData("Type");
            bool isGlobal = (type & 0x4) != 0;
            return isGlobal ? 1 : 0;
        }
    }
}
