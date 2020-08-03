using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class VertexFormat : FormatDef {
        protected virtual int vertex => 0;

        public VertexFormat(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        // TODO: sortKey
        // TODO: warnings?
        // TODO: display value?

        public override string DataToValue(ValueElement element, dynamic data) {
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }
    }

    public class Vertex0Format : VertexFormat {
        public static string defType = "Vertex0Format";

        public Vertex0Format(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }
    }

    public class Vertex1Format : VertexFormat {
        public static string defType = "Vertex1Format";
        protected override int vertex => 1;

        public Vertex1Format(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}
    }

    public class Vertex2Format : VertexFormat {
        public static string defType = "Vertex2Format";
        protected override int vertex => 2;

        public Vertex2Format(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }
    }
}
