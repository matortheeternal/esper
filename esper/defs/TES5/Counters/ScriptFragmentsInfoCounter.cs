using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class ScriptFragmentsInfoCounter : CounterDef {
        public static readonly string defId = "ScriptFragmentsInfoCounter";

        public ScriptFragmentsInfoCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }

        // TODO: SetCount?

        public override UInt32 GetCount(Container container) {
            UInt32 count = 0;
            UInt64 flags = container.GetData("Flags");
            for (int i = 0; i < 8; i++) {
                if ((flags & (UInt64)1 << i) != 1) continue;
                count++;
                // TODO: warn when i >= 3
            }
            return count;
        }
    }
}
