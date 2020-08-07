using Newtonsoft.Json.Linq;
using esper.elements;
using esper.parsing;
using esper.setup;
using System;

namespace esper.defs {
    public class StringDef : ValueDef {
        public static string defType = "string";
        private int? prefix => src.Value<int?>("prefix");
        private int? padding => src.Value<int?>("padding");
        private bool localized => src.Value<bool>("localized");
        private bool keepCase => src.Value<bool>("keepCase");
        protected new bool isVariableSize => prefix == null && size == null;

        public StringDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            if (localized && source.localized)
                return source.ReadLocalizedString();
            // dataSize - 1 because null terminator
            int? size = this.size ?? 
                (int?) source.ReadPrefix(prefix, padding) ?? 
                (dataSize != null ? dataSize - 1 : null);
            return source.ReadString(size);
        }

        public override dynamic DefaultData() {
            return "";
        }

        public override void SetData(ValueElement element, dynamic data) {
            string s = data;
            if (s == null) 
                throw new Exception("Data must be a string");
            if (size != null && s.Length != size)
                throw new Exception("String length does not match");
            element._data = data;
        }

        public override string GetValue(ValueElement element) {
            return element.data;
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = value;
        }
    }
}
