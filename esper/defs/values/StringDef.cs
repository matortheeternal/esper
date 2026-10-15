using esper.data;
using esper.elements;
using esper.io;
using esper.setup;

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

        internal StringDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            prefix = src.Value<int?>("prefix");
            padding = src.Value<int?>("padding");
            localized = src.Value<bool>("localized");
            keepCase = src.Value<bool>("keepCase");
        }

        internal override ValueData ReadData(DataSource source, UInt32? dataSize) {
            if (localized && source.localized)
                return LocalizedString.Read(source);
            // dataSize - 1 because null terminator
            int? size = fixedSize ??
                (int?)source.ReadPrefix(prefix, padding) ??
                (dataSize != null ? (int?)(dataSize - 1) : null);
            return source.ReadString(size);
        }

        internal override ValueData DefaultData() {
            return string.Empty;
        }

        internal override void SetData(ValueElement element, ValueData data) {
            string str = (data as ValueData.Text)?.V;
            if (str == null)
                throw new Exception("Data must be a string");
            if (!isVariableSize && str.Length != fixedSize)
                throw new Exception("String length does not match");
            element._data = str;
        }

        public override string GetValue(ValueElement element) {
            if (element.data is ValueData.Text text) return text.V;
            if (element.data is ValueData.LString lstring)
                return lstring.V.ToString(element);
            return null;
        }

        public override void SetValue(ValueElement element, string value) {
            SetData(element, value);
        }

        internal override string DataToSortKey(ValueData data) {
            string str = data switch {
                null => null,
                ValueData.LString l => l.V.ToString(),
                ValueData.Text t => t.V,
                _ => throw new InvalidOperationException("Data must be a string."),
            };
            if (str == null) return string.Empty;
            return keepCase ? str : str.ToUpper();
        }

        internal override UInt32 GetSize(Element element) {
            UInt32 size = base.GetSize(element);
            if (fixedSize != null) return size;
            var v = (ValueElement)element;
            if (v.data is ValueData.LString) {
                return size + 4;
            } else {
                var s = v.data is ValueData.Text t ? t.V : string.Empty;
                if (prefix != null) size += (UInt32)prefix;
                if (padding != null) size += (UInt32)padding;
                if (isVariableSize) size += 1;
                return (UInt32)(size + s.Length);
            }
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is ValueData.LString l) {
                l.V.WriteTo(output);
            } else {
                var s = element.data is ValueData.Text t ? t.V : string.Empty;
                if (prefix != null)
                    output.WritePrefix(s.Length, (int)prefix, padding ?? 0);
                output.WriteString(s, isVariableSize);
            }
        }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            if (!isBase) return src;
            src.Add("type", defId);
            if (prefix != null) src.Add("prefix", prefix);
            if (padding != null) src.Add("padding", padding);
            if (localized) src.Add("localized", localized);
            if (keepCase) src.Add("keepCase", keepCase);
            return src;
        }
    }
}
