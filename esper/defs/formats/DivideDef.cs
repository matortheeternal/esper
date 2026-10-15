using esper.elements;
using esper.setup;

namespace esper.defs {
    public class DivideDef : FormatDef {
        public static readonly string defId = "divide";
        public int divisionValue;

        internal DivideDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            divisionValue = src.Value<int>("value");
        }

        internal override string DataToValue(ValueElement element, VariantData data) {
            float f = data.ToSingle() / (float)divisionValue;
            return f.ToString(sessionOptions.floatFormat);
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            return float.Parse(value) * divisionValue;
        }
    }
}
