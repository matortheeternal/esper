using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs.values {
    public class Int16Def : ValueDef {
        public static readonly string defType = "int16";
        public new int size { get => 2; }

        public Int16Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public new DataContainer ReadData(PluginFileSource source) {
            Int16 data = source.reader.ReadInt16();
            return new IntData<Int16>(data);
        }

        public new DataContainer DefaultData() {
            return new IntData<Int16>(0);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new IntData<Int16>(Int16.Parse(value));
        }
    }
}