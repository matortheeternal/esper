using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Int32Def : ValueDef {
        public static readonly string defType = "int32";
        public new int size { get => 4; }

        public Int32Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public new DataContainer ReadData(PluginFileSource source) {
            Int32 data = source.reader.ReadInt32();
            return new IntData<Int32>(data);
        }

        public new DataContainer DefaultData() {
            return new IntData<Int32>(0);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new IntData<Int32>(Int32.Parse(value));
        }
    }
}