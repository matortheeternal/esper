using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class ActorValueFormat : FormatDef {
        public static string defType = "EPFDActorValueFormat";

        private static EnumDef actorValueEnum;

        public ActorValueFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            var enumSrc = manager.ResolveDef("ActorValueEnum");
            actorValueEnum = (EnumDef) manager.BuildDef(enumSrc, this);
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            byte[] bytes = BitConverter.GetBytes(data);
            float fData = BitConverter.ToSingle(bytes);
            long index = (long)Math.Round(fData);
            return actorValueEnum.DataToValue(element, index);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            float fData = actorValueEnum.ValueToData(element, value);
            byte[] bytes = BitConverter.GetBytes(fData);
            return BitConverter.ToUInt32(bytes);
        }
    }
}
