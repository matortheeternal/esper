using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class Int16Def : ValueDef {
        public static readonly string defId = "int16";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 2;

        internal Int16Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override ValueData ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadInt16();
        }

        internal override ValueData DefaultData() {
            return (Int16)0;
        }

        internal override string DataToSortKey(ValueData data) {
            UInt32 v = data is ValueData.I16 i16 ? (UInt32)(Int32)i16.V : (UInt32)data.ToUInt64();
            v += (UInt32)(data.ToInt64() + Math.Abs((Int32)Int16.MinValue));
            return v.ToString("X4");
        }

        internal override void SetData(ValueElement element, ValueData data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt16(data.ToInt64())
                : data is ValueData.I16 i16 ? i16.V : (Int16)data.ToInt64();
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is ValueData.I16 data) {
                output.writer.Write(data.V);
            } else {
                output.writer.Write((short)0);
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