using esper.elements;
using esper.plugins;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;
using esper.helpers;

namespace esper.defs {
    public class Int16Def : ValueDef {
        public static readonly string defType = "int16";
        public override int? size => 2;

        public Int16Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadInt16();
        }

        public override dynamic DefaultData() {
            return (Int16)0;
        }

        public override string DataToSortKey(dynamic data) {
            UInt32 v = (UInt32)data;
            v += (UInt32) Math.Abs(Int16.MinValue);
            return v.ToString("X4");
        }

        public override void SetData(ValueElement element, dynamic data) {
            element._data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToInt16(data)
                : (Int16)data;
            element.SetState(ElementState.Modified);
        }
    }
}