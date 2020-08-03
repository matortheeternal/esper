using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class EdgeFormat : FormatDef {
        protected virtual int edge => 0;

        public EdgeFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

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
        public static string defType = "Vertex0Format";

        public Edge0Format(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }
    }

    public class Edge1Format : EdgeFormat {
        public static string defType = "Vertex1Format";
        protected override int edge => 1;

        public Edge1Format(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }
    }

    public class Edge2Format : EdgeFormat {
        public static string defType = "Vertex2Format";
        protected override int edge => 2;

        public Edge2Format(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }
    }
}
