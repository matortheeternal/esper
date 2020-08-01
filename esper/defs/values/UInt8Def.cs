using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class UInt8Def : ValueDef {
        public static readonly string defType = "uint8";
        public override int? size => 1;

        public UInt8Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadByte();
        }

        public override dynamic DefaultData() {
            return (byte)0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element.data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt8(data)
                : (byte)data;
            element.SetState(ElementState.Modified);
        }
    }
}
