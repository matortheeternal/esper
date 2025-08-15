using esper.elements;
using esper.io;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;
using esper.helpers;

namespace esper.defs {
    public class UInt16Def : ValueDef {
        public static readonly string defId = "uint16";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 2;

        public UInt16Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
            if (dataSize == 1) return (UInt16)source.reader.ReadByte();
            return source.reader.ReadUInt16();
        }

        public override dynamic DefaultData() {
            return (UInt16)0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt16(data)
                : (UInt16)data;
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is UInt16 data) {
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
