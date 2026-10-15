using esper.elements;
using esper.setup;

namespace esper.defs {
    public class FormatDef : Def {
        public virtual bool customSortKey => false;
        public virtual bool isNumeric => false;
        public override XEDefType defType => XEDefType.dtIntegerFormater;

        internal FormatDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal virtual string DataToValue(ValueElement element, object data) {
            throw new NotImplementedException();
        }

        internal virtual object ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        internal virtual string GetSortKey(ValueElement element, object data) {
            return DataToValue(element, data);
        }
    }
}
