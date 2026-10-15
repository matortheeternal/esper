using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class CTDATypeFormat : FormatDef {
        public static readonly string defId = "CtdaTypeFormat";
        private static readonly Regex valueExpr = new Regex(@"$([\w ]+?)(?: / ([\w ]+))?");

        private static FlagsDef ctdaTypeFlags;
        private static EnumDef ctdaTypeEnum;

        internal CTDATypeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {
            ctdaTypeFlags = (FlagsDef)manager.ResolveDef("CtdaTypeFlags");
            ctdaTypeEnum = (EnumDef)manager.ResolveDef("CtdaTypeEnum");
        }

        internal override string DataToValue(ValueElement element, ValueData data) {
            long d = data.ToInt64();
            string type = ctdaTypeEnum.DataToValue(element, d & 0xE0);
            string flags = ctdaTypeFlags.DataToValue(element, d & 0x1F);
            return flags != "" ? $"{type} / {flags}" : type;
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            var match = valueExpr.Match(value);
            if (match == null) return 0;
            var enumData = ctdaTypeEnum.ValueToData(element, match.Groups[1].Value);
            var flagsData = match.Groups.Count > 2
                ? ctdaTypeFlags.ValueToData(element, match.Groups[2].Value)
                : 0;
            return enumData.ToInt64() + flagsData.ToInt64();
        }
    }
}
