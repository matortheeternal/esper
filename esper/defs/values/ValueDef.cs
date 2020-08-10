using esper.elements;
using esper.plugins;
using esper.setup;
using esper.helpers;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ValueDef : MaybeSubrecordDef {
        public FormatDef formatDef;

        public override int? size => src.Value<int?>("size");
        public bool zeroSortKey => src.Value<bool>("zeroSortKey");
        protected bool isVariableSize => size == null;

        public ValueDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            formatDef = JsonHelpers.FormatDef(src, this);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? size = null
        ) {
            return new ValueElement(container, this, true) {
                _data = ReadData(source, size)
            };
        }

        public override Element InitElement(Container container) {
            return new ValueElement(container, this);
        }

        public virtual dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            throw new NotImplementedException();
        }

        public virtual dynamic DefaultData() {
            throw new NotImplementedException();
        }

        public virtual void SetData(ValueElement element, dynamic data) {
            element._data = data;
            element.SetState(ElementState.Modified);
        }

        public virtual string DataToString(dynamic data) {
            return data.ToString();
        }

        public virtual string DataToSortKey(dynamic data) {
            return data.ToString($"X{2 * size}");
        }

        public virtual string GetValue(ValueElement element) {
            if (formatDef == null) return DataToString(element.data);
            return formatDef.DataToValue(element, element.data);
        }

        public virtual void SetValue(ValueElement element, string value) {
            SetData(element, formatDef == null
                ? Int64.Parse(value)
                : formatDef.ValueToData(element, value));
        }

        public override string GetSortKey(Element element) {
            var v = (ValueElement)element;
            string sortKey = formatDef == null
                ? DataToSortKey(v.data)
                : formatDef.GetSortKey(v, v.data);
            return zeroSortKey
                ? new string('0', sortKey.Length)
                : sortKey;
        }
    }
}
