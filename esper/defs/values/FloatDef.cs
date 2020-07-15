using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class FloatDef : ValueDef {
        public static string defType = "float";

        public new int size { get => 4; }

        public FloatDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {}

        public new DataContainer ReadData(PluginFileSource source) {
            return new FloatData(source);
        }

        public new DataContainer DefaultData() {
            return new FloatData(0.0f);
        }

        public new void SetValue(ValueElement element, string value) {
            float data = Single.Parse(value);
            element.data = new FloatData(data);
        }
    }
}
