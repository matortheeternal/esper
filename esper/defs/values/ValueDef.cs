using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class ValueDef : Def {
        public ValueDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {
        }

        public Element Build(Container container) {
            return new ValueElement(container, this);
        }

        public DataContainer GetData(ValueElement element) {
            return element.data;
        }

        public void SetData(ValueElement element, DataContainer data) {
            element.data = data;
            element.SetState(ElementState.Modified);
        }

        public string GetValue(ValueElement element) {
            return element.data.ToString();
        }
    }
}
