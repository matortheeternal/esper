using esper.elements;
using esper.setup;
using System.Globalization;

namespace esper.defs {
    public class ScaledInt4Format : FormatDef {
        public static readonly string defId = "ScaledInt4Format";
        public override bool customSortKey => true;
        public override bool isNumeric => true;

        internal ScaledInt4Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, VariantData data) {
            double f = data.ToInt64() / 10000.0;
            return f.ToString("F4", CultureInfo.InvariantCulture);
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            double f = double.Parse(value, CultureInfo.InvariantCulture);
            return (long)Math.Round(f * 10000);
        }

        internal override string GetSortKey(ValueElement element, VariantData data) {
            var s = DataToValue(element, data).PadLeft(22, '0');
            return (data.ToInt64() < 0 ? "-" : "+") + s;
        }
    }
}
