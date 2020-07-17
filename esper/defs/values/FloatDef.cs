using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class FloatDef : ValueDef {
        public static string defType = "float";

        public new int size { get => 4; }

        public FloatDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override dynamic ReadData(PluginFileSource source) {
            return source.reader.ReadSingle();
        }

        public override dynamic DefaultData() {
            return 0.0f;
        }

        public override string GetValue(ValueElement element) {
            float data = element.data;
            return data.ToString("N5");
        }

        public override void SetValue(ValueElement element, string value) {
            float data = Single.Parse(value);
            element.data = data;
        }
    }
}
