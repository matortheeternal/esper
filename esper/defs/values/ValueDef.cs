using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ValueDef : Def {
        public int size {
            get {
                if (!src.ContainsKey("size")) return 0;
                return src.Value<int>("size");
            }
        }
        public bool isVariableSize { get => size == 0; }

        public ValueDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
        }

        public override Element ReadElement(Container container, PluginFileSource source) {
            return new ValueElement(container, this, source);
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

        public virtual dynamic ReadData(PluginFileSource source) {
            throw new NotImplementedException();
        }
    }
}
