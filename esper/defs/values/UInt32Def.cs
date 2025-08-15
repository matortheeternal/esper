using esper.elements;
using esper.io;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;
using esper.helpers;

namespace esper.defs {
    public class UInt32Def : ValueDef {
        public static readonly string defId = "uint32";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override SmashType smashType => SmashType.stInteger;
        public override bool isNumeric => formatDef == null || formatDef.isNumeric;

        public override int? size => 4;

        public UInt32Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public UInt32Def(UInt32Def other) : base(other) {}

        public override dynamic ReadData(DataSource source, UInt32? size) {
            return source.reader.ReadUInt32();
        }

        public override dynamic DefaultData() {
            return 0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt32(data)
                : (UInt32)data;
            element.SetState(ElementState.Modified);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is UInt32 data) {
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