using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class UInt8Def : ValueDef {
        public static readonly string defType = "uint8";
        public new int size { get => 1; }

        public UInt8Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return source.reader.ReadByte();
        }

        public override dynamic DefaultData() {
            return 0;
        }

        public override string GetValue(ValueElement element) {
            sbyte data = element.data;
            return data.ToString();
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = byte.Parse(value);
        }
    }
}
