using esper.elements;
using esper.setup;

namespace esper.defs.FO3 {
    public class WthrDataClassificationFormat : FormatDef {
        public static readonly string defId = "WthrDataClassificationFormat";
        public override bool customSortKey => true;

        internal WthrDataClassificationFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            long v = data.ToInt64() & ~192L;
            switch (v) {
                case 0: return "None";
                case 1: return "Pleasant";
                case 2: return "Cloudy";
                case 4: return "Rainy";
                case 8: return "Snow";
                default: return "<Unknown: " + v + ">";
            }
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            return data.ToInt64().ToString("X2");
        }
    }
}
