using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class RotationFactorFormat : FormatDef {
        public static string defType = "RotationFactorFormat";

        public RotationFactorFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {}

        // TODO: normalize
        public override string DataToValue(ValueElement element, dynamic data) {
            float fData = data;
            return (fData * 180 / Math.PI).ToString("F4");
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            float fData = float.Parse(value);
            return fData * Math.PI / 180;
        }
    }
}
