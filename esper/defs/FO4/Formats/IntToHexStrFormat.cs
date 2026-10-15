using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.FO4 {
    public class IntToHexStrFormat : FormatDef {
        public static readonly string defId = "IntToHexStrFormat";

        internal IntToHexStrFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            return Convert.ToUInt32(data.ToInt64()).ToString("X8");
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return Fo4FormatHelpers.WbHexStrToInt(value);
        }
    }
}
