using esper.elements;
using esper.helpers;
using esper.setup;

namespace esper.defs {
    public class MultiplyDef : FormatDef {
        public static readonly string defId = "multiply";
        public int factor;

        internal MultiplyDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            factor = src.Value<int>("value");
        }

        internal override string DataToValue(ValueElement element, dynamic data) {
            return (Convert.ToInt64(data) * factor).ToString();
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseInt64(value) / factor;
        }
    }
}
