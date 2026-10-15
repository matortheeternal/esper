using esper.elements;
using esper.helpers;
using esper.setup;

namespace esper.defs {
    public class FormatUnion : FormatDef {
        public static readonly string defId = "formatUnion";
        public override XEDefType defType => XEDefType.dtIntegerFormaterUnion;

        public Decider decider;
        public List<FormatDef> formatDefs;

        internal FormatUnion(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {
            decider = JsonHelpers.Decider(manager, src);
            formatDefs = JsonHelpers.Defs<FormatDef>(manager, src, "formats");
        }

        public FormatDef ResolveDef(Container container) {
            var index = decider.Decide(container);
            return formatDefs[index];
        }

        internal override string DataToValue(ValueElement element, object data) {
            var resolvedDef = ResolveDef(element.container as Container);
            return resolvedDef.DataToValue(element, data);
        }

        internal override object ValueToData(ValueElement element, string value) {
            var resolvedDef = ResolveDef(element.container as Container);
            return resolvedDef.ValueToData(element, value);
        }
    }
}
