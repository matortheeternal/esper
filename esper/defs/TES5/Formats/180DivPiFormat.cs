using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class _180DivPiFormat : FormatDef {
        public static readonly string defId = "180DivPiFormat";
        public override bool isNumeric => true;

        public _180DivPiFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string DataToValue(ValueElement element, dynamic data) {
            if (Single.IsNaN(data)) return "NaN";
            if (Single.IsInfinity(data)) return "Inf";
            if (data == Single.MaxValue) return "Default";
            if (data == -Single.MaxValue) return "Min";
            float fData = data * 180 / Math.PI;
            return fData.ToString(sessionOptions.floatFormat);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
