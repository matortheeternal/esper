using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class UInt32Def : ValueDef {
        public static readonly string defType = "uint32";
        public new int size { get => 4; }

        public UInt32Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public new UInt32 ReadData(PluginFileSource source) {
            return source.reader.ReadUInt32();
        }

        public new UInt32 DefaultData() {
            return 0;
        }

        public new string GetValue(ValueElement element) {
            UInt32 data = element.data;
            return data.ToString();
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = UInt32.Parse(value);
        }
    }
}