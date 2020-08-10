using Newtonsoft.Json.Linq;
using esper.elements;
using esper.data;
using esper.plugins;
using esper.setup;
using System;

namespace esper.defs {
    public class StringDef : ValueDef {
        public static string defType = "string";

        private readonly int? prefix;
        private readonly int? padding;
        private readonly bool localized;
        private readonly bool keepCase;

        protected override bool isVariableSize => prefix == null && fixedSize == null;

        public StringDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            prefix = src.Value<int?>("prefix");
            padding = src.Value<int?>("padding");
            localized = src.Value<bool>("localized");
            keepCase = src.Value<bool>("keepCase");
    }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            if (localized && source.localized)
                return source.ReadLocalizedString();
            // dataSize - 1 because null terminator
            int? size = this.fixedSize ?? 
                (int?) source.ReadPrefix(prefix, padding) ?? 
                (dataSize != null ? dataSize - 1 : null);
            return source.ReadString(size);
        }

        public override dynamic DefaultData() {
            return string.Empty;
        }

        public override void SetData(ValueElement element, dynamic data) {
            string str = (string) data;
            if (str == null) 
                throw new Exception("Data must be a string");
            if (!isVariableSize && str.Length != fixedSize)
                throw new Exception("String length does not match");
            element._data = str;
        }

        public override string GetValue(ValueElement element) {
            if (element.data is string str) return str;
            if (element.data is LocalizedString lstring)
                return lstring.ToString();
            return null;
        }

        public override void SetValue(ValueElement element, string value) {
            SetData(element, value);
        }

        public override string DataToSortKey(dynamic data) {
            string str = data is LocalizedString lstring
                ? lstring.ToString()
                : (string) data;
            if (str == null) return string.Empty;
            return keepCase ? str : str.ToUpper();
        }
    }
}
