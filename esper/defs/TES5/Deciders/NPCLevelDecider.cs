using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class NPCLevelDecider : Decider {
        public override int Decide(Container container) {
            var e = container.GetElement("Flags");
            if (e == null) return 0;
            return (e.GetData() & 0x80) != 0 ? 1 : 0;
        }
    }
}
