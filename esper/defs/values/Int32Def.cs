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
            : base(manager, src, parent) {}

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadInt32();
        }

        public override dynamic DefaultData() {
            return 0;
        }

        public override string GetValue(ValueElement element) {
            Int32 data = element.data;
            return data.ToString();
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = Int32.Parse(value);
        }
    }
}