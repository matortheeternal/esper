using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class Int8Def : ValueDef {
        public static readonly string defId = "int8";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 1;

        internal Int8Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override object ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadSByte();
        }

        internal override object DefaultData() {
            return (sbyte)0;
        }

        internal override string DataToSortKey(object data) {
            Int32 v = (Int32)data.ToInt64();
            v += Math.Abs((Int32)sbyte.MinValue);
            return v.ToString("X2");
        }

        internal override void SetData(ValueElement element, object data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt8(data.ToInt64())
                : (sbyte)data.ToInt64();
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is sbyte data) {
                output.writer.Write(data);
            } else {
                output.writer.Write((sbyte)0);
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