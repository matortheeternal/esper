using esper.elements;
using esper.setup;

namespace esper.defs.FO4 {
    public class CELLCombinedRefsCounter : ElementCounter {
        public new static readonly string defId = "CELLCombinedRefsCounter";

        internal CELLCombinedRefsCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src, "References Count") { }

        public override UInt32 GetCount(Container container) {
            return base.GetCount(container) / 2;
        }

        public override void SetCount(Container container, UInt32 count) {
            base.SetCount(container, count * 2);
        }
    }
}
