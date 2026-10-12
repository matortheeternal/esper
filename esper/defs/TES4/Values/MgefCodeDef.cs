using esper.setup;

namespace esper.defs.TES4 {
    public class MgefCodeDef : StringDef {
        public new static readonly string defId = "mgefCode";

        internal MgefCodeDef(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }
    }
}
