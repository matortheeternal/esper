using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class EdgeFormat : FormatDef {
        protected virtual int edge => 0;

        public EdgeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        // TODO: sortKey
        // TODO: warnings?
        // TODO: display value?

        public override string DataToValue(ValueElement element, dynamic data) {
            if (data < 0) return "";
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            if (value == "" || value == "None") return -1;
            return Int64.Parse(value);
        }
    }

    public class Edge0Format : EdgeFormat {
        public static string defType = "Edge0Format";

        public Edge0Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Edge1Format : EdgeFormat {
        public static string defType = "Edge1Format";
        protected override int edge => 1;

        public Edge1Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Edge2Format : EdgeFormat {
        public static string defType = "Edge2Format";
        protected override int edge => 2;

        public Edge2Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
