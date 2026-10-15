using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class ActorValueFormat : FormatDef {
        public static readonly string defId = "EPFDActorValueFormat";
        public override bool customSortKey => true;

        private static EnumDef actorValueEnum;

        internal ActorValueFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {
            var enumSrc = manager.ResolveDefSource("ActorValueEnum");
            actorValueEnum = (EnumDef)manager.BuildDef(enumSrc);
        }

        private long GetIndex(VariantData data) {
            float fData = BitConverter.ToSingle(data.ToBytes());
            return (long)Math.Round(fData);
        }

        internal override string DataToValue(ValueElement element, VariantData data) {
            long index = GetIndex(data);
            return actorValueEnum.DataToValue(element, index);
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            float fData = actorValueEnum.ValueToData(element, value).ToSingle();
            byte[] bytes = BitConverter.GetBytes(fData);
            return BitConverter.ToUInt32(bytes);
        }

        internal override string GetSortKey(ValueElement element, VariantData data) {
            long index = GetIndex(data);
            return actorValueEnum.GetSortKey(element, index);
        }
    }
}
