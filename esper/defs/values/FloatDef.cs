using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class FloatDef : ValueDef {
        public static string defType = "float";
        private static float epsilon = 0.000009999999999f;

        public override int? size => 4;

        public FloatDef(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadSingle();
        }

        public override dynamic DefaultData() {
            return 0.0f;
        }

        public override string DataToSortKey(dynamic data) {
            if (Single.IsNaN(data)) return new string(' ', 23);
            if (data == Single.MaxValue) return "+" + new string('9', 22);
            string sortKey = Math.Abs(data).ToString("99N5").PadLeft(22, '0');
            bool isNegative = (data < 0) && (Math.Abs(data) > epsilon);
            return (isNegative ? '-' : '+') + sortKey;
        }

        public override string DataToString(dynamic data) {
            return data.ToString("N5");
        }

        public override void SetValue(ValueElement element, string value) {
            float data = Single.Parse(value);
            element.data = data;
        }
    }
}
