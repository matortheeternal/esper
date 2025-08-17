using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class UInt8Def : ValueDef {
        public static readonly string defId = "uint8";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 1;

        public UInt8Def(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
            return source.reader.ReadByte();
        }

        public override dynamic DefaultData() {
            return (byte)0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt8(data)
                : (byte)data;
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is byte data) {
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
