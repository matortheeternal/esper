using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbScriptFragmentsEmptyScriptDecider: fragments without
    // a script name hold null instead of script data.
    public class ScriptFragmentsEmptyScriptDecider : Decider {
        public override int Decide(Container container) {
            var scriptName = container?.GetValue("ScriptName");
            if (scriptName == null) return 0;
            return scriptName == "" ? 1 : 0;
        }
    }
}
