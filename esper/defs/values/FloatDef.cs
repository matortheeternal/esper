using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class FloatDef : ValueDef {
        public static readonly string defType = "float";

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
            string sortKey = Math.Abs(data)
                .ToString(sessionOptions.floatFormat)
                .PadLeft(22, '0');
            bool isNegative = (data < 0) && (Math.Abs(data) > sessionOptions.epsilon);
            return (isNegative ? '-' : '+') + sortKey;
        }

        public override string DataToString(dynamic data) {
            if (Single.IsNaN(data)) return "NaN";
            if (Single.IsInfinity(data)) return "Inf";
            if (data == Single.MaxValue) return "Default";
            if (data == -Single.MaxValue) return "Min";
            return data.ToString(sessionOptions.floatFormat);
        }

        public override void SetValue(ValueElement element, string value) {
            float data = Single.Parse(value);
            element.data = data;
        }
    }
}
