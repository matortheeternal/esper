using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class TimeFormat : FormatDef {
        private static readonly Regex timeExpr = new Regex(
            @"^([0-1][0-9]|2[0-3]):([0-5][0-9])$"
        );

        public static readonly string defId = "ClmtTimeFormat";

        internal TimeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, VariantData data) {
            Int64 time = data.ToInt64();
            var hours = time / 6;
            var minutes = (time % 6) * 10;
            if (hours > 23) return time.ToString();
            return $"{hours:00}:{minutes:00}";
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            var match = timeExpr.Match(value);
            if (match == null) return 0;
            var hours = int.Parse(match.Groups[1].Value);
            var minutes = int.Parse(match.Groups[2].Value);
            var time = (hours * 6) + (minutes / 10);
            return (byte)time;
        }
    }
}
