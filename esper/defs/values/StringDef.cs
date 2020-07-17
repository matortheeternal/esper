using Newtonsoft.Json.Linq;
using esper.elements;
using esper.parsing;
using esper.setup;

namespace esper.defs {
    // TODO: handle strings with prefixes
    public class StringDef : ValueDef {
        public static string defType = "string";

        public StringDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override dynamic ReadData(PluginFileSource source) {
            return source.ReadString(size);
        }

        public override dynamic DefaultData() {
            return "";
        }

        public override string GetValue(ValueElement element) {
            string data = element.data;
            return data;
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = value;
        }
    }
}
