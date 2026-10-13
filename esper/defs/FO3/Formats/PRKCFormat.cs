using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FO3 {
    public class PRKCFormat : FormatDef {
        public static readonly string defId = "PRKCFormat";
        public override bool customSortKey => true;

        internal PRKCFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var entryPoint = element?.container?.container?.container
                ?.GetData<int?>(@"DATA\Entry Point\Entry Point");
            if (entryPoint == null) return data.ToString();
            var ep = (int)entryPoint;
            if (ep < 0 || ep >= PerkTables.EntryPointConditions.Length)
                return data.ToString();
            var condition = PerkTables.EntryPointConditions[ep];
            var count = PerkTables.ConditionCounts[condition];
            var captions = PerkTables.ConditionCaptions[condition];
            if (data < 0 || data >= count) return data.ToString();
            return captions[data];
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return ((long)data).ToString("X2");
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            if (long.TryParse(value.Trim(), out var number)) return number;
            if (value.Trim() == "") return 0;
            var entryPoint = element?.container?.container?.container
                ?.GetData<int?>(@"DATA\Entry Point\Entry Point");
            if (entryPoint != null) {
                var ep = (int)entryPoint;
                if (ep >= 0 && ep < PerkTables.EntryPointConditions.Length) {
                    var condition = PerkTables.EntryPointConditions[ep];
                    var captions = PerkTables.ConditionCaptions[condition];
                    for (var i = 0; i < captions.Length; i++) {
                        if (string.Equals(captions[i], value.Trim(), System.StringComparison.OrdinalIgnoreCase))
                            return i;
                    }
                }
            }
            return DataHelpers.ParseInt64(value);
        }
    }
}
