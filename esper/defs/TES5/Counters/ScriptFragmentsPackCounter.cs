using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class ScriptFragmentsPackCounter : CounterDef {
        public static string defType = "ScriptFragmentsPackCounter";

        public ScriptFragmentsPackCounter(
            DefinitionManager manager, JObject src, Def parent
        ) : base(manager, src, parent) { }

        // TODO: SetCount?

        public override UInt32 GetCount(Container container) {
            UInt32 count = 0;
            var flags = container.GetData("Flags");
            for (int i = 0; i < 8; i++) {
                if (flags & (1 << i) != 1) continue;
                count++;
            }
            return count;
        }
    }
}
