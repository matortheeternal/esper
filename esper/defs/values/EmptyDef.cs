using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class EmptyDef : ValueDef {
        public static string defType = "empty";

        public override int? size => 0;

        public EmptyDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override string GetValue(ValueElement element) {
            return "";
        }

        public override void SetValue(ValueElement element, string value) {}

        public override dynamic DefaultData() {
            return null;
        }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return null;
        }

        public override string GetSortKey(Element element) {
            return "";
        }
    }
}
