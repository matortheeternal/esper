using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class BOOKTeachesDecider : Decider {
        public override int Decide(Container container) {
            var flagsElement = container?.GetElement("Flags") as ValueElement;
            if (flagsElement == null) return 0;
            long flags = Convert.ToInt64(flagsElement.data);
            return ((flags & 0x4) != 0) ? 1 : 0;
        }
    }
}
