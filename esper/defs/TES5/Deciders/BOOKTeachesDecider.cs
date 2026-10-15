using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class BOOKTeachesDecider : Decider {
        public override int Decide(Container container) {
            var flagsElement = container?.GetElement("Flags") as ValueElement;
            if (flagsElement == null) return 0;
            long flags = flagsElement.data?.ToInt64() ?? 0;
            return ((flags & 0x4) != 0) ? 1 : 0;
        }
    }
}
