using esper.elements;
using esper.resolution;

namespace esper.defs.TES4 {
    public class GMSTUnionDecider : Decider {
        public override int Decide(Container container) {
            string edid = container?.GetValue("EDID");
            if (String.IsNullOrEmpty(edid)) return 1;
            return edid[0] switch {
                's' => 0,
                'f' => 2,
                _ => 1
            };
        }
    }
}
