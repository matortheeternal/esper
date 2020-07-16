using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Int16Def : ValueDef {
        public static readonly string defType = "int16";
        public new int size { get => 2; }

        public Int16Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public new Int16 ReadData(PluginFileSource source) {
            return source.reader.ReadInt16();
        }

        public new Int16 DefaultData() {
            return 0;
        }

        public new string GetValue(ValueElement element) {
            Int16 data = element.data;
            return data.ToString();
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = Int16.Parse(value);
        }
    }
}