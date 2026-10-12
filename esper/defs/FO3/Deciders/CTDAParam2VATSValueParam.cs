using esper.elements;
using esper.resolution;

namespace esper.defs.FO3 {
    // Port of xEdit 4.0.4 wbCTDAParam2VATSValueParamDecider (referenced in
    // esp.json without the Decider suffix): indexes the union by the native
    // value of the sibling 'Parameter #1' element.
    public class CTDAParam2VATSValueParam : Decider {
        public override int Decide(Container container) {
            var param1 = container?.GetElement("Parameter #1") as ValueElement;
            if (param1 == null) return 0;
            return Convert.ToInt32(param1.data);
        }
    }
}
