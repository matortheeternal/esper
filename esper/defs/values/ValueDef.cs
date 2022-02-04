using esper.elements;
using esper.io;
using esper.setup;
using esper.helpers;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ValueDef : MaybeSubrecordDef {
        public override XEDefType defType => IsSubrecord()
            ? XEDefType.dtSubRecord : valueDefType;
        public virtual XEDefType valueDefType => throw new NotImplementedException();

        public FormatDef formatDef;
        public int? fixedSize;
        private readonly bool zeroSortKey;

        public override int? size => fixedSize;
        protected virtual bool isVariableSize => fixedSize == null;

        public ValueDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            formatDef = JsonHelpers.FormatDef(manager, src);
            fixedSize = src.Value<int?>("size");
            zeroSortKey = src.Value<bool>("zeroSortKey");
        }

        public ValueDef(ValueDef other) : base(other) {
            formatDef = other.formatDef;
            fixedSize = other.fixedSize;
            zeroSortKey = other.zeroSortKey;
        }

        public override Element ReadElement(
            Container container, DataSource source, UInt32? size = null
        ) {
            return new ValueElement(container, this) {
                _data = ReadData(source, size)
            };
        }

        public override Element NewElement(Container container = null) {
            return new ValueElement(container, this);
        }

        public virtual dynamic ReadData(DataSource source, UInt32? dataSize) {
            throw new NotImplementedException();
        }

        public virtual dynamic DefaultData() {
            throw new NotImplementedException();
        }

        public virtual void SetData(ValueElement element, dynamic data) {
            element._data = data;
            element.MarkModified();
        }

        public virtual string DataToString(dynamic data) {
            return data.ToString();
        }

        public virtual string DataToSortKey(dynamic data) {
            return data.ToString($"X{2 * size}");
        }

        public virtual string GetValue(ValueElement element) {
            if (element.data == null) return "";
            if (formatDef == null) return DataToString(element.data);
            return formatDef.DataToValue(element, element.data);
        }

        public virtual void SetValue(ValueElement element, string value) {
            SetData(element, formatDef == null
                ? DataHelpers.ParseInt64(value)
                : formatDef.ValueToData(element, value));
        }

        public override string GetSortKey(Element element) {
            var v = (ValueElement)element;
            string sortKey = formatDef != null && formatDef.customSortKey
                ? formatDef.GetSortKey(v, v.data)
                : DataToSortKey(v.data);
            return zeroSortKey
                ? new string('0', sortKey.Length)
                : sortKey;
        }

        internal override UInt32 GetSize(Element element) {
            return (UInt32) (base.GetSize(element) + (size ?? 0));
        }

        internal virtual void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            throw new NotImplementedException();
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            base.WriteElement(element, output);
            WriteData((ValueElement)element, output);
        }
    }
}
