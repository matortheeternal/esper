using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FO3 {
    public class PerkEPFTFormat : FormatDef {
        public static readonly string defId = "PerkEPFTFormat";
        public override bool customSortKey => true;

        internal PerkEPFTFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var function = element?.container?.container
                ?.GetData<int?>(@"DATA\Entry Point\Function");
            if (function == null) return data.ToString();
            var fn = (int)function;
            if (fn < 0 || fn >= PerkTables.FunctionParamTypes.Length)
                return data.ToString();
            if (data < 0 || data >= PerkTables.FunctionParamNames.Length)
                return data.ToString();
            var text = PerkTables.FunctionParamNames[data];
            if (data != PerkTables.FunctionParamTypes[fn])
                return text + " <Warning: Value out of Bounds for this Function>";
            return text;
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return ((long)data).ToString("X2");
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            if (long.TryParse(value.Trim(), out var number)) return number;
            for (var i = 0; i < PerkTables.FunctionParamNames.Length; i++) {
                if (string.Equals(
                    PerkTables.FunctionParamNames[i],
                    value.Trim(),
                    System.StringComparison.OrdinalIgnoreCase
                )) {
                    return i;
                }
            }
            return DataHelpers.ParseInt64(value);
        }
    }
}
