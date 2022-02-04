using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace esper.defs.TES5 {
    public class CTDATypeFormat : FormatDef {
        public static readonly string defId = "CtdaTypeFormat";
        private static readonly Regex valueExpr = new Regex(@"$([\w ]+?)(?: / ([\w ]+))?");

        private static FlagsDef ctdaTypeFlags;
        private static EnumDef ctdaTypeEnum;

        public CTDATypeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {
            ctdaTypeFlags = (FlagsDef) manager.ResolveDef("CtdaTypeFlags");
            ctdaTypeEnum = (EnumDef) manager.ResolveDef("CtdaTypeEnum");
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            long d = data;
            string type = ctdaTypeEnum.DataToValue(element, d & 0xE0);
            string flags = ctdaTypeFlags.DataToValue(element, d & 0x1F);
            return flags != "" ? $"{type} / {flags}" : type;
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            var match = valueExpr.Match(value);
            if (match == null) return 0;
            var enumData = ctdaTypeEnum.ValueToData(element, match.Groups[1].Value);
            var flagsData = match.Groups.Count > 2
                ? ctdaTypeFlags.ValueToData(element, match.Groups[2].Value)
                : 0;
            return enumData + flagsData;
        }
    }
}
