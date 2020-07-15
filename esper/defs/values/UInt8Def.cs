using esper.data;
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

        public new DataContainer ReadData(PluginFileSource source) {
            byte data = source.reader.ReadByte();
            return new IntData<byte>(data);
        }

        public new DataContainer DefaultData() {
            return new IntData<byte>(0);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new IntData<byte>(byte.Parse(value));
        }
    }
}
