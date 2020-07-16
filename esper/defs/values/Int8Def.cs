using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Int8Def : ValueDef {
        public static readonly string defType = "int8";
        public new int size { get => 1; }

        public Int8Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public new sbyte ReadData(PluginFileSource source) {
            return source.reader.ReadSByte();
        }

        public new sbyte DefaultData() {
            return 0;
        }

        public new string GetValue(ValueElement element) {
            sbyte data = element.data;
            return data.ToString();
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = sbyte.Parse(value);
        }
    }
}