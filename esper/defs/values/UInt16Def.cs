using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class UInt16Def : ValueDef {
        public static readonly string defType = "uint16";
        public new int size { get => 2; }

        public UInt16Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public new UInt16 ReadData(PluginFileSource source) {
            return source.reader.ReadUInt16();
        }

        public new UInt16 DefaultData() {
            return 0;
        }

        public new string GetValue(ValueElement element) {
            UInt16 data = element.data;
            return data.ToString();
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = UInt16.Parse(value);
        }
    }
}
