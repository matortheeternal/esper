using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class UInt16Def : ValueDef {
        public static readonly string defId = "uint16";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 2;

        internal UInt16Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override ValueData ReadData(DataSource source, UInt32? dataSize) {
            if (dataSize == 1) return (UInt16)source.reader.ReadByte();
            return source.reader.ReadUInt16();
        }

        internal override ValueData DefaultData() {
            return (UInt16)0;
        }

        internal override void SetData(ValueElement element, ValueData data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt16(data.ToInt64())
                : data is ValueData.U16 u16 ? u16.V : (UInt16)data.ToUInt64();
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is ValueData.U16 data) {
                output.writer.Write(data.V);
            } else {
                output.writer.Write((ushort)0);
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
