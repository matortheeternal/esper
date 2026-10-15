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

        private long GetIndex(ValueData data) {
            byte[] bytes = data switch {
                ValueData.F32 f => BitConverter.GetBytes(f.V),
                ValueData.I32 i => BitConverter.GetBytes(i.V),
                ValueData.U32 u => BitConverter.GetBytes(u.V),
                ValueData.I64 l => BitConverter.GetBytes(l.V),
                _ => throw new InvalidOperationException("Expected a numeric value."),
            };
            float fData = BitConverter.ToSingle(bytes);
            return (long)Math.Round(fData);
        }

        internal override string DataToValue(ValueElement element, ValueData data) {
            long index = GetIndex(data);
            return actorValueEnum.DataToValue(element, index);
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            float fData = actorValueEnum.ValueToData(element, value).ToSingle();
            byte[] bytes = BitConverter.GetBytes(fData);
            return BitConverter.ToUInt32(bytes);
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            long index = GetIndex(data);
            return actorValueEnum.GetSortKey(element, index);
        }
    }
}
