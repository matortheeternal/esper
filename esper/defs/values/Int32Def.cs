using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;
using esper.helpers;

namespace esper.defs {
    public class Int32Def : ValueDef {
        public static readonly string defType = "int32";
        public override int? size => 4;

        public Int32Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadInt32();
        }

        public override dynamic DefaultData() {
            return 0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt32(data)
                : (Int32)data;
            element.SetState(ElementState.Modified);
        }
    }
}