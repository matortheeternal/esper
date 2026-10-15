using esper.elements;
using esper.setup;

namespace esper.defs.FO3 {
    public class WthrDataClassificationFormat : FormatDef {
        public static readonly string defId = "WthrDataClassificationFormat";
        public override bool customSortKey => true;

        internal WthrDataClassificationFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, object data) {
            long v = Convert.ToInt64(data) & ~192L;
            switch (v) {
                case 0: return "None";
                case 1: return "Pleasant";
                case 2: return "Cloudy";
                case 4: return "Rainy";
                case 8: return "Snow";
                default: return "<Unknown: " + v + ">";
            }
        }

        internal override object ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        internal override string GetSortKey(ValueElement element, object data) {
            return Convert.ToInt64(data).ToString("X2");
        }
    }
}
