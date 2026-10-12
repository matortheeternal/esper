using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    public class ScriptPropertyDecider : Decider {
        public override int Decide(Container container) {
            byte type = container?.GetData<byte?>("Type") ?? 0;
            if (type <= 7) return type;
            if (type >= 11 && type <= 15) return type - 3;
            if (type == 17) return 13;
            return 0;
        }
    }
}
