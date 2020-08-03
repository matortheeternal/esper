using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class EPFDDecider : Decider {
        public override int Decide(Container container) {
            var epft = container?.GetData("EPFT");
            if (epft != 2) return epft ?? 0;
            var function = container.GetData(@"..\DATA\Entry Point\Function");
            return function switch {
                5 => 8, 12 => 8, 13 => 8, 14 => 8,
                _ => 2
            };
        }
    }
}
