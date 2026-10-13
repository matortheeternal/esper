using esper.elements;
using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace esper.defs.FNV {
    public class AlocTimeFormat : FormatDef {
        public static readonly string defId = "AlocTimeFormat";
        public override bool customSortKey => true;

        internal AlocTimeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var seconds = (long)Math.Round(Convert.ToDouble(data) / 256.0 * 86400.0);
            var hour24 = (int)(seconds / 3600) % 24;
            var hour12 = hour24 % 12;
            if (hour12 == 0) hour12 = 12;
            return $"{hour12}:{(int)(seconds / 60) % 60:00}:{(int)seconds % 60:00} {(hour24 < 12 ? "AM" : "PM")}";
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            if (DateTime.TryParseExact(value.Trim(), new[] { "h:mm:ss tt", "H:mm:ss" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                return (byte)Math.Clamp((int)Math.Round(time.TimeOfDay.TotalDays * 256.0), 0, 255);
            return DataHelpers.ParseInt64(value);
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return Convert.ToInt64(data).ToString("X4");
        }
    }
}
