using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class UInt8Def : ValueDef {
        public static readonly string defType = "uint8";
        public new int size { get => 1; }

        public UInt8Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public new byte ReadData(PluginFileSource source) {
            return source.reader.ReadByte();
        }

        public new byte DefaultData() {
            return 0;
        }

        public new string GetValue(ValueElement element) {
            sbyte data = element.data;
            return data.ToString();
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = byte.Parse(value);
        }
    }
}
