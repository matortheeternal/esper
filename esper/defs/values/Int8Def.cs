using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class Int8Def : ValueDef {
        public static readonly string defType = "int8";
        public new int size { get => 1; }

        public Int8Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public new DataContainer ReadData(PluginFileSource source) {
            sbyte data = source.reader.ReadSByte();
            return new IntData<sbyte>(data);
        }

        public new DataContainer DefaultData() {
            return new IntData<sbyte>(0);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new IntData<sbyte>(sbyte.Parse(value));
        }
    }
}