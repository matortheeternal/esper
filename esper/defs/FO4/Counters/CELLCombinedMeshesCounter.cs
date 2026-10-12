using esper.setup;

namespace esper.defs.FO4 {
    public class CELLCombinedMeshesCounter : ElementCounter {
        public new static readonly string defId = "CELLCombinedMeshesCounter";

        internal CELLCombinedMeshesCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src, "Meshes Count") { }
    }
}
