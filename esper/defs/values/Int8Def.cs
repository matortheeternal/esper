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

        internal override VariantData ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadSByte();
        }

        internal override VariantData DefaultData() {
            return (sbyte)0;
        }

        internal override string DataToSortKey(VariantData data) {
            Int32 v = data.ToInt32();
            v += Math.Abs((Int32)sbyte.MinValue);
            return v.ToString("X2");
        }

        internal override void SetData(ValueElement element, VariantData data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt8(data.ToInt64())
                : data.ToSByte();
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is VariantData.I8 data) {
                output.writer.Write(data.V);
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