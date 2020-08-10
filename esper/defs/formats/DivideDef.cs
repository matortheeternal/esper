using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class DivideDef : FormatDef {
        public static string defType = "divide";
        public int divisionValue;

        public DivideDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            divisionValue = src.Value<int>("value");
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            float f = data / divisionValue;
            return f.ToString("N5");
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return float.Parse(value) * divisionValue;
        }
    }
}
