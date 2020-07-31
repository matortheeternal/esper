using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class ScriptPropertyDecider : Decider {
        public override int Decide(Container container) {
            if (container == null) return 0;
            byte type = container.GetData("Type");
            if (type <= 5) return type;
            if (type >= 11 && type <= 15) return type - 5;
            return 0;
        }
    }
}
