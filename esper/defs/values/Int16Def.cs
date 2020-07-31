using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

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
            return 0;
        }

        public override string GetValue(ValueElement element) {
            Int16 data = element.data;
            if (formatDef == null) return data.ToString();
            return formatDef.DataToValue(element, data);
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = Int16.Parse(value);
        }
    }
}