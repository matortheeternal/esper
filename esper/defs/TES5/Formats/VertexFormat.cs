using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class VertexFormat : FormatDef {
        protected virtual int vertex => 0;
        public override bool customSortKey => true;

        public VertexFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        // TODO: resolve vertex
        // TODO: warnings?
        // TODO: display value?

        public override string DataToValue(ValueElement element, dynamic data) {
            return data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }

        public override string GetSortKey(ValueElement element, dynamic data) {
            UInt16 v = data;
            return $"{v:X4}";
        }
    }

    public class Vertex0Format : VertexFormat {
        public static readonly string defId = "Vertex0Format";

        public Vertex0Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Vertex1Format : VertexFormat {
        public static readonly string defId = "Vertex1Format";
        protected override int vertex => 1;

        public Vertex1Format(DefinitionManager manager, JObject src)
            : base(manager, src) {}
    }

    public class Vertex2Format : VertexFormat {
        public static readonly string defId = "Vertex2Format";
        protected override int vertex => 2;

        public Vertex2Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
