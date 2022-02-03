using Newtonsoft.Json.Linq;
using esper.elements;
using esper.data;
using esper.plugins;
using esper.setup;
using System;

namespace esper.defs {
    public class StringDef : ValueDef {
        public static readonly string defId = "string";
        public override XEDefType valueDefType {
            get {
                if (localized) return XEDefType.dtLString;
                if (prefix != null) return XEDefType.dtLenString;
                return XEDefType.dtString;
            }
        }
        public override SmashType smashType => SmashType.stString;

        private readonly int? prefix;
        private readonly int? padding;
        private readonly bool localized;
        private readonly bool keepCase;

        protected override bool isVariableSize => prefix == null && fixedSize == null;

        public StringDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            prefix = src.Value<int?>("prefix");
            padding = src.Value<int?>("padding");
            localized = src.Value<bool>("localized");
            keepCase = src.Value<bool>("keepCase");
    }

        public override dynamic ReadData(PluginFileSource source, UInt32? dataSize) {
            if (localized && source.localized)
                return LocalizedString.Read(source);
            // dataSize - 1 because null terminator
            int? size = fixedSize ?? 
                (int?)source.ReadPrefix(prefix, padding) ?? 
                (dataSize != null ? (int?) (dataSize - 1) : null);
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
                return lstring.ToString(element);
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

        internal override UInt32 GetSize(Element element) {
            UInt32 size = base.GetSize(element);
            if (fixedSize != null) return size;
            var v = (ValueElement)element;
            if (v.data is LocalizedString) {
                return size + 4;
            } else {
                var s = v.data as string ?? DefaultData();
                if (prefix != null) size += (UInt32)prefix;
                if (padding != null) size += (UInt32)padding;
                if (isVariableSize) size += 1;
                return (UInt32) (size + s.Length);
            }
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is LocalizedString ls) {
                ls.WriteTo(output);
            } else {
                var s = element.data as string ?? DefaultData();
                if (prefix != null)
                    output.WritePrefix(s.Length, (int)prefix, padding ?? 0);
                output.WriteString(s, isVariableSize);
            }
        }
    }
}
