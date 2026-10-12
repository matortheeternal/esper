using esper.setup;

namespace esper.defs.FO4 {
    public class OMODDataIncludeCounter : ElementCounter {
        public new static readonly string defId = "OMODDataIncludeCounter";

        internal OMODDataIncludeCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src, "Include Count") { }
    }
}
