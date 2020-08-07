using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class OffsetDataColsCounter : CounterDef {
        public static string defType = "OffsetDataColsCounter";

        public OffsetDataColsCounter(
            DefinitionManager manager, JObject src, Def parent
        ) : base(manager, src, parent) {}

        public override UInt32 GetCount(Container container) {
            var bounds = container.GetElement(@"..\Object Bounds");
            float? minX = bounds?.GetData(@"NAM0\X");
            float? maxX = bounds?.GetData(@"NAM9\X");
            if (minX == null || maxX == null) return 0;
            if (minX >= int.MaxValue || minX <= 0) return 0;
            var max = (int.MaxValue - minX + 1);
            if (maxX >= max || maxX <= 1) return 1;
            return (UInt32) (Math.Truncate((float) maxX) - minX + 1);
        }
    }
}
