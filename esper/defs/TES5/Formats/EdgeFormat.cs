using esper.elements;
using esper.setup;

namespace esper.defs.TES5 {
    public class EdgeFormat : FormatDef {
        protected virtual int edge => 0;
        public override bool customSortKey => true;

        internal EdgeFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        // TODO: resolve edge link
        // TODO: warnings?

        internal override string DataToValue(ValueElement element, object data) {
            //if (data < 0) return "";
            return data.ToString();
        }

        internal override object ValueToData(ValueElement element, string value) {
            if (value == "" || value == "None") return -1;
            return Int64.Parse(value);
        }

        internal override string GetSortKey(ValueElement element, object data) {
            UInt16 v = (UInt16)data.ToUInt64();
            return $"00000000{v:X4}";
        }
    }

    public class Edge0Format : EdgeFormat {
        public static readonly string defId = "Edge0Format";

        internal Edge0Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Edge1Format : EdgeFormat {
        public static readonly string defId = "Edge1Format";
        protected override int edge => 1;

        internal Edge1Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class Edge2Format : EdgeFormat {
        public static readonly string defId = "Edge2Format";
        protected override int edge => 2;

        internal Edge2Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
