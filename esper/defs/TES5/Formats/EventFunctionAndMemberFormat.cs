using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class EventFunctionAndMemberFormat : FormatDef {

        public static string defType = "EventFunctionAndMemberFormat";

        private readonly EnumDef eventFunctionEnum;
        private readonly EnumDef eventMemberEnum;

        public EventFunctionAndMemberFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {
            eventFunctionEnum = (EnumDef) manager.ResolveDef("EventFunctionEnum");
            eventMemberEnum = (EnumDef) manager.ResolveDef("EventMemberEnum");
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            Int64 n = (Int64) data;
            var eventFunction = eventFunctionEnum.DataToValue(element, n & 0xFFFF);
            var eventMember = eventMemberEnum.DataToValue(element, n >> 16);
            return $"{eventFunction}:{eventMember}";
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
