using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class Int8Def : ValueDef {
        public static readonly string defType = "int8";
        public override int? size => 1;

        public Int8Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadSByte();
        }

        public override dynamic DefaultData() {
            return (sbyte)0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt8(data)
                : (sbyte)data;
            element.SetState(ElementState.Modified);
        }
    }
}