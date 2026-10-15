using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class RotationFactorFormat : FormatDef {
        public static readonly string defId = "RotationFactorFormat";
        public override bool isNumeric => true;

        public RotationFactorFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }

        // TODO: normalize
        internal override string DataToValue(ValueElement element, object data) {
            float fData = data.ToSingle();
            return (fData * 180 / Math.PI).ToString("F4");
        }

        internal override object ValueToData(ValueElement element, string value) {
            float fData = float.Parse(value);
            return fData * Math.PI / 180;
        }
    }
}
