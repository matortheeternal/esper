using esper.setup;

namespace esper.defs.FO4 {
    public class OMODDataPropertyCounter : ElementCounter {
        public new static readonly string defId = "OMODDataPropertyCounter";

        internal OMODDataPropertyCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src, "Property Count") { }
    }
}
