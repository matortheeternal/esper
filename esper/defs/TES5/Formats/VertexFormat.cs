using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class VertexFormat : FormatDef {
        public override bool isNumeric => true;
        protected virtual int vertex => 0;
        public override bool customSortKey => true;

        internal VertexFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        // TODO: resolve vertex
        // TODO: warnings?
        // TODO: display value?

        internal override string DataToValue(ValueElement element, ValueData data) {
            return data.ToDisplayString();
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            UInt16 v = data.ToUInt16();
            return $"{v:X4}";
        }
    }

    public class Vertex0Format : VertexFormat {
        public static readonly string defId = "Vertex0Format";

        internal Vertex0Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Vertex1Format : VertexFormat {
        public static readonly string defId = "Vertex1Format";
        protected override int vertex => 1;

        internal Vertex1Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Vertex2Format : VertexFormat {
        public static readonly string defId = "Vertex2Format";

        protected override int vertex => 2;

        internal Vertex2Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
