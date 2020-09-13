using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class ActorValueFormat : FormatDef {
        public static readonly string defId = "EPFDActorValueFormat";
        public override bool customSortKey => true;

        private static EnumDef actorValueEnum;

        public ActorValueFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {
            var enumSrc = manager.ResolveDefSource("ActorValueEnum");
            actorValueEnum = (EnumDef) manager.BuildDef(enumSrc);
        }

        private long GetIndex(dynamic data) {
            byte[] bytes = BitConverter.GetBytes(data);
            float fData = BitConverter.ToSingle(bytes);
            return (long)Math.Round(fData);
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            long index = GetIndex(data);
            return actorValueEnum.DataToValue(element, index);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            float fData = actorValueEnum.ValueToData(element, value);
            byte[] bytes = BitConverter.GetBytes(fData);
            return BitConverter.ToUInt32(bytes);
        }

        public override string GetSortKey(ValueElement element, dynamic data) {
            long index = GetIndex(data);
            return actorValueEnum.GetSortKey(element, index);
        }
    }
}
