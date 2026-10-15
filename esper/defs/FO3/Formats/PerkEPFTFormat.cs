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

        internal override string DataToValue(ValueElement element, VariantData data) {
            var function = element?.container?.container
                ?.GetData<int?>(@"DATA\Entry Point\Function");
            if (function == null) return data.ToDisplayString();
            var fn = (int)function;
            if (fn < 0 || fn >= PerkTables.FunctionParamTypes.Length)
                return data.ToDisplayString();
            if (data.ToInt64() < 0 || data.ToInt64() >= PerkTables.FunctionParamNames.Length)
                return data.ToDisplayString();
            var text = PerkTables.FunctionParamNames[(int)data.ToInt64()];
            if (data.ToInt64() != PerkTables.FunctionParamTypes[fn])
                return text + " <Warning: Value out of Bounds for this Function>";
            return text;
        }

        internal override string GetSortKey(ValueElement element, VariantData data) {
            return data.ToInt64().ToString("X2");
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
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
