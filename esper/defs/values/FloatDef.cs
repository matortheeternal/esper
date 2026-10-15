using esper.elements;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class FloatDef : ValueDef {
        public static readonly string defId = "float";
        public override XEDefType valueDefType => XEDefType.dtFloat;
        public override SmashType smashType => SmashType.stFloat;
        public override bool isNumeric => true;

        public override int? size => 4;

        internal FloatDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override VariantData ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadSingle();
        }

        internal override VariantData DefaultData() {
            return 0.0f;
        }

        internal override string DataToSortKey(VariantData data) {
            if (Single.IsNaN(data.ToSingle())) return new string(' ', 23);
            if (data.ToSingle() == Single.MaxValue) return "+" + new string('9', 22);
            string sortKey = Math.Abs(data.ToSingle())
                .ToString(sessionOptions.floatFormat)
                .PadLeft(22, '0');
            bool isNegative = (data.ToSingle() < 0) && (Math.Abs(data.ToSingle()) > sessionOptions.epsilon);
            return (isNegative ? '-' : '+') + sortKey;
        }

        internal override string DataToString(VariantData data) {
            if (Single.IsNaN(data.ToSingle())) return "NaN";
            if (Single.IsInfinity(data.ToSingle())) return "Inf";
            if (data.ToSingle() == Single.MaxValue) return "Default";
            if (data.ToSingle() == -Single.MaxValue) return "Min";
            return data.ToSingle().ToString(sessionOptions.floatFormat);
        }

        public override void SetValue(ValueElement element, string value) {
            float data = Single.Parse(value);
            element.data = data;
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is VariantData.F32 data) {
                output.writer.Write(data.V);
            } else {
                output.writer.Write(0.0f);
            }
        }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            if (!isBase) return src;
            src.Add("type", defId);
            return src;
        }
    }
}
