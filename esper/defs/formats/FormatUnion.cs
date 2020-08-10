using esper.elements;
using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class FormatUnion : FormatDef {
        public static string defType = "formatUnion";

        public Decider decider;
        public ReadOnlyCollection<FormatDef> formatDefs;

        public FormatUnion(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {
            decider = JsonHelpers.Decider(src, this);
            formatDefs = JsonHelpers.FormatDefs(src, this);
        }

        public FormatDef ResolveDef(Container container) {
            var index = decider.Decide(container);
            return formatDefs[index];
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            throw new NotImplementedException();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }
    }
}
