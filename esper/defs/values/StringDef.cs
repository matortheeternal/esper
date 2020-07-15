using Newtonsoft.Json.Linq;
using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;

namespace esper.defs {
    // TODO: handle strings with prefixes
    public class StringDef : ValueDef {
        public static string defType = "string";

        public StringDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public new DataContainer ReadData(PluginFileSource source) {
            return new StringData(source, size);
        }

        public new DataContainer DefaultData() {
            return new StringData("");
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new StringData(value);
        }
    }
}
