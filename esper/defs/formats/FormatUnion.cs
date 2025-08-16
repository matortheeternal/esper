using esper.elements;
using esper.helpers;
using esper.setup;

namespace esper.defs {
    [JSExport]
    public class FormatUnion : FormatDef {
        public static readonly string defId = "formatUnion";
        public override XEDefType defType => XEDefType.dtIntegerFormaterUnion;

        public Decider decider;
        public List<FormatDef> formatDefs;

        public FormatUnion(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {
            decider = JsonHelpers.Decider(manager, src);
            formatDefs = JsonHelpers.Defs<FormatDef>(manager, src, "formats");
        }

        public FormatDef ResolveDef(Container container) {
            var index = decider.Decide(container);
            return formatDefs[index];
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            var resolvedDef = ResolveDef(element.container);
            return resolvedDef.DataToValue(element, data);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            var resolvedDef = ResolveDef(element.container);
            return resolvedDef.ValueToData(element, value);
        }
    }
}
