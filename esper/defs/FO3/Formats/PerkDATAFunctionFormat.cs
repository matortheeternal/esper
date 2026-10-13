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

        internal override string DataToValue(ValueElement element, dynamic data) {
            var entryPoint = element?.container?.GetData<int?>("Entry Point");
            if (entryPoint == null) return data.ToString();
            var ep = (int)entryPoint;
            if (ep < 0 || ep >= PerkTables.EntryPointFunctionTypes.Length)
                return data.ToString();
            if (data < 0 || data >= PerkTables.FunctionNames.Length)
                return data.ToString();
            var text = PerkTables.FunctionNames[data];
            if (PerkTables.FunctionTypes[data] != PerkTables.EntryPointFunctionTypes[ep])
                return text + " <Warning: Value out of Bounds for this Entry Point>";
            return text;
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return ((long)data).ToString("X2");
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            if (long.TryParse(value.Trim(), out var number)) return number;
            for (var i = 0; i < PerkTables.FunctionNames.Length; i++) {
                if (string.Equals(PerkTables.FunctionNames[i], value.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return DataHelpers.ParseInt64(value);
        }
    }
}
