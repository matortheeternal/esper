using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class CTDAParam2VATSValueParamDecider : Decider {
        public override int Decide(Container container) {
            var param1 = container?.GetElement("Parameter #1") as ValueElement;
            return param1 == null ? 0 : Convert.ToInt32(param1.data);
        }
    }
}
