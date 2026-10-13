using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace esper.defs.FO4 {
    public class CLFMColorFormat : FormatDef {
        public static readonly string defId = "CLFMColorFormat";
        public override bool customSortKey => true;

        internal CLFMColorFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var flags = element?.GetData<uint?>(@"..\FNAM") ?? 0;
            var raw = Convert.ToUInt32(data);
            if ((flags & 2) != 0)
                return BitConverter.ToSingle(
                    BitConverter.GetBytes(raw), 0
                ).ToString("F6", CultureInfo.InvariantCulture);
            return "rgba(" + (raw & 0xFF) +
                ", " + ((raw >> 8) & 0xFF) +
                ", " + ((raw >> 16) & 0xFF) +
                ", " + ((raw >> 24) & 0xFF) + ")";
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return Convert.ToUInt32(data).ToString("X8");
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            value ??= "";
            if (value.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase)) {
                var end = value.IndexOf(')');
                if (end < 0) return 0u;
                var parts = value.Substring(5, end - 5).Split(',');
                if (parts.Length != 4) return 0u;
                uint result = 0;
                for (var i = 0; i < 4; i++) {
                    if (!int.TryParse(parts[i], out var channel)) channel = 0;
                    result |= (uint)(byte)channel << (i * 8);
                }
                return result;
            }
            if (!float.TryParse(
                value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var parsed
            )) parsed = 0;
            return BitConverter.ToUInt32(BitConverter.GetBytes(parsed), 0);
        }
    }
}
