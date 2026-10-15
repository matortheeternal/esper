using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class _180DivPiFormat : FormatDef {
        public static readonly string defId = "180DivPiFormat";
        public override bool isNumeric => true;

        internal _180DivPiFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            if (Single.IsNaN(data.ToSingle())) return "NaN";
            if (Single.IsInfinity(data.ToSingle())) return "Inf";
            if (data.ToSingle() == Single.MaxValue) return "Default";
            if (data.ToSingle() == -Single.MaxValue) return "Min";
            float fData = (float)(data.ToSingle() * 180 / Math.PI);
            return fData.ToString(sessionOptions.floatFormat);
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
