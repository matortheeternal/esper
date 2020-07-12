using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class FloatDef : ValueDef {
        public static string defType = "float";

        public int size { get => 4; }

        public FloatDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {}

        public DataContainer ReadData(PluginFileSource source) {
            return new FloatData(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserFloatData(value);
        }
    }
}
