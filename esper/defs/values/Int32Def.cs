using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class Int32Def : ValueDef {
        public static readonly string defId = "int32";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 4;

        internal Int32Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override ValueData ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadInt32();
        }

        internal override ValueData DefaultData() {
            return 0;
        }

        internal override string DataToSortKey(ValueData data) {
            UInt32 v = (UInt32)data.ToUInt64();
            v += (UInt32)Math.Abs((Int64)Int32.MinValue);
            return v.ToString("X8");
        }

        internal override void SetData(ValueElement element, ValueData data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt32(data.ToInt64())
                : data.ToInt32();
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is ValueData.I32 data) {
                output.writer.Write(data.V);
            } else {
                output.writer.Write(0);
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