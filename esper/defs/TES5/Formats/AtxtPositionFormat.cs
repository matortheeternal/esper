using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class AtxtPositionFormat : FormatDef {
        public static readonly string defId = "AtxtPositionFormat";
        public override bool customSortKey => true;

        internal AtxtPositionFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            UInt16 v = data is ValueData.U16 u ? u.V : (UInt16)data.ToUInt64();
            return $"{v} -> {v / 17}:{v % 17}";
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            UInt16 v = data is ValueData.U16 u ? u.V : (UInt16)data.ToUInt64();
            return $"{v / 17:X2}{v % 17:X2}";
        }
    }
}
