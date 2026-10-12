using esper.elements;

namespace esper.defs.TES4 {
    // Port of xEdit 4.0.4 wbMISCActorValueDecider: if the containing record
    // has flags & 0xC0 set, the union holds an Actor Value, else Unknown.
    public class MISCActorValueDecider : Decider {
        public override int Decide(Container container) {
            var rec = container?.record;
            if (rec == null) return 0;
            return (rec.header.flags & 0xC0) == 0xC0 ? 1 : 0;
        }
    }
}
