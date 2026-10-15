using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FO3 {
    public class NVTREdgeFormat : FormatDef {
        public static readonly string defId = "NVTREdgeFormat";
        public override bool customSortKey => true;

        internal NVTREdgeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            var name = element?.def?.name;
            var index = name != null && name.Length > 0 ? name[0] - '0' : -1;
            if (index < 0 || index > 2) return data.ToDisplayString();
            var flags = element?.GetData<int?>(@"..\..\Flags");
            if (flags == null) return data.ToDisplayString();
            if ((flags & (1 << index)) == 0) return data.ToDisplayString();
            var tri = (int)data.ToInt64();
            var navm = element?.record;
            var connection = navm?.GetElements("NVEX")?.Skip(tri).FirstOrDefault();
            var mesh = connection?.GetElement("Navigation Mesh");
            var target = (mesh as ValueElement)?.referencedRecord;
            var triangle = connection?.GetData<int?>("Triangle");
            if (target == null || triangle == null)
                return $"#{data.ToDisplayString()} (Triangle #{tri})";
                return "#" + data.ToDisplayString() + " (Triangle #" +
                    triangle + " in \"" + target.GetValue("EDID") + "\")";
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            return "";
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseLeadingUInt(value);
        }
    }
}
