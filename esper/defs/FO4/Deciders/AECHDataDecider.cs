using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbAECHDataDecider: indexes the union by the effect's
    // KNAM Type value (BSOverdrive -> 0, BSStateVariableFilter -> 1,
    // BSDelayEffect -> 2, anything else -> 0).
    public class AECHDataDecider : Decider {
        public override int Decide(Container container) {
            long? type = (long?)container?.GetData<long?>(@"..\KNAM");
            if (type == null) return 0;
            switch (type) {
                case 2252866750: return 0; // BSOverdrive
                case 4015480703: return 1; // BSStateVariableFilter
                case 411269967: return 2; // BSDelayEffect
                default: return 0;
            }
        }
    }
}
