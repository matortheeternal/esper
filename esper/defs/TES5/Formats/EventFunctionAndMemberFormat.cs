using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class EventFunctionAndMemberFormat : FormatDef {

        public static string defType = "EventFunctionAndMemberFormat";

        public EventFunctionAndMemberFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }

        public override string DataToValue(ValueElement element, dynamic data) {
            // TODO
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
