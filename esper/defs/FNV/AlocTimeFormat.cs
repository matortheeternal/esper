using esper.elements;
using esper.setup;

namespace esper.defs.FNV {
    public class AlocTimeFormat : FormatDef {
        public static readonly string defId = "AlocTimeFormat";
        public override bool customSortKey => true;

        internal AlocTimeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var t = TimeSpan.FromDays(Convert.ToDouble(data) / 256.0);
            return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return Convert.ToInt64(data).ToString("X4");
        }
    }
}
