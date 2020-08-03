using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class GMSTUnionDecider : Decider {
        public override int Decide(Container container) {
            string edid = container.GetValue("EDID");
            return edid?[0] switch {
                's' => 0, // string
                'f' => 2, // float
                'b' => 3, // boolean
                _ => 1    // int32
            };
        }
    }
}
