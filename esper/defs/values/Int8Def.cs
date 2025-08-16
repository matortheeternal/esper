using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    [JSExport]
    public class Int8Def : ValueDef {
        public static readonly string defId = "int8";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 1;

        public Int8Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadSByte();
        }

        public override dynamic DefaultData() {
            return (sbyte)0;
        }

        public override string DataToSortKey(dynamic data) {
            Int32 v = (Int32)data;
            v += Math.Abs((Int32) sbyte.MinValue);
            return v.ToString("X2");
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt8(data)
                : (sbyte)data;
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is sbyte data) {
                output.writer.Write(data);
            } else {
                output.writer.Write(DefaultData());
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