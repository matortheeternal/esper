using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class UInt32Def : ValueDef {
        public static readonly string defId = "uint32";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 4;

        internal UInt32Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public UInt32Def(UInt32Def other) : base(other) { }

        internal override ValueData ReadData(DataSource source, UInt32? size) {
            return source.reader.ReadUInt32();
        }

        internal override ValueData DefaultData() {
            return 0;
        }

        internal override void SetData(ValueElement element, ValueData data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt32(data.ToUInt64())
                : data.ToUInt32();
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is ValueData.U32 data) {
                output.writer.Write(data.V);
            } else {
                output.writer.Write(0u);
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