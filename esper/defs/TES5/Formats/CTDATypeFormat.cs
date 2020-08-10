using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class CTDATypeFormat : FormatDef {
        public static string defType = "CtdaTypeFormat";

        private static FlagsDef ctdaTypeFlags;

        public CTDATypeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {
            var flagsSrc = manager.ResolveDefSource("CtdaTypeFlags");
            ctdaTypeFlags = (FlagsDef) manager.BuildDef(flagsSrc);
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            string type = (data & 0xE0) switch {
                0x00 => "Equal to",
                0x20 => "Not equal to",
                0x40 => "Greater than",
                0x60 => "Greater than or equal to",
                0x80 => "Less than",
                0xA0 => "Less than or equal to",
                _ => "<Unknown Compare operator>"
            };
            string flags = ctdaTypeFlags.DataToValue(element, data & 0x1F);
            return flags != "" ? $"{type} / {flags}" : type;
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
