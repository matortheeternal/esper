using Newtonsoft.Json.Linq;
using esper.elements;
using esper.parsing;
using esper.setup;

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

        private int ReadPrefixSize(PluginFileSource source) {
            int size = 0;
            switch(prefix) {
                case 1: size = source.reader.ReadByte(); break;
                case 2: size = source.reader.ReadUInt16(); break;
                case 4: size = (int)source.reader.ReadUInt32(); break;
            }
            if (padding != null) source.reader.ReadBytes((int)padding);
            return size;
        }

        private int? GetStringSize(PluginFileSource source) {
            if (prefix != null) return ReadPrefixSize(source);
            return size;
        }

        public override dynamic ReadData(PluginFileSource source) {
            if (localized && source.localized)
                return source.ReadLocalizedString();
            var size = GetStringSize(source);
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
