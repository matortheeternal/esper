using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FO3 {
    public class PerkDATAFunctionFormat : FormatDef {
        public static readonly string defId = "PerkDATAFunctionFormat";
        public override bool customSortKey => true;

        internal PerkDATAFunctionFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, object data) {
            var entryPoint = element?.container?.GetData<int?>("Entry Point");
            if (entryPoint == null) return data.ToDisplayString();
            var ep = (int)entryPoint;
            if (ep < 0 || ep >= PerkTables.EntryPointFunctionTypes.Length)
                return data.ToDisplayString();
            if (data.ToInt64() < 0 || data.ToInt64() >= PerkTables.FunctionNames.Length)
                return data.ToDisplayString();
            var text = PerkTables.FunctionNames[(int)data.ToInt64()];
            if (PerkTables.FunctionTypes[(int)data.ToInt64()] != PerkTables.EntryPointFunctionTypes[ep])
                return text + " <Warning: Value out of Bounds for this Entry Point>";
            return text;
        }

        internal override string GetSortKey(ValueElement element, object data) {
            return data.ToInt64().ToString("X2");
        }

        internal override object ValueToData(ValueElement element, string value) {
            if (long.TryParse(value.Trim(), out var number)) return number;
            for (var i = 0; i < PerkTables.FunctionNames.Length; i++) {
                if (string.Equals(
                    PerkTables.FunctionNames[i],
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
