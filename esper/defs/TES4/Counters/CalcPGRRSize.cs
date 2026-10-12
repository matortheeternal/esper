using esper.elements;
using esper.resolution;
using esper.setup;

namespace esper.defs.TES4 {
    public class CalcPGRRSize : CounterDef {
        public static readonly string defId = "CalcPGRRSize";

        internal CalcPGRRSize(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }

        public override UInt32 GetCount(Container container) {
            if (container?.container == null) return 0;
            var points = container.container.GetElement("Points") as Container;
            int pointIndex = Math.Max(container.count - 1, 0);
            var point = points?.GetElement($"[{pointIndex}]") as Container;
            var connections = point?.GetElement("Connections") as ValueElement;
            return connections == null ? 0 : Convert.ToUInt32(connections.data);
        }
    }
}
