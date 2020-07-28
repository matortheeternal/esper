using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ValueDef : MaybeSubrecordDef {
        public FormatDef formatDef;
        public override int? size => src.Value<int?>("size");
        protected bool isVariableSize { get => size == null; }

        public ValueDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            var format = src.Value<JObject>("format");
            if (format == null) return;
            formatDef = (FormatDef) manager.BuildDef(format, this);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? size = null
        ) {
            return new ValueElement(container, this, true) {
                data = ReadData(source, size)
            };
        }

        public override Element InitElement(Container container) {
            return new ValueElement(container, this);
        }

        public virtual dynamic GetData(ValueElement element) {
            return element.data;
        }

        public virtual void SetData(ValueElement element, dynamic data) {
            element.data = data;
            element.SetState(ElementState.Modified);
        }

        public virtual string GetValue(ValueElement element) {
            throw new NotImplementedException();
        }

        public virtual void SetValue(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        public virtual dynamic DefaultData() {
            throw new NotImplementedException();
        }

        public virtual dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            throw new NotImplementedException();
        }
    }
}
