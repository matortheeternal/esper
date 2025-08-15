using esper.elements;
using esper.io;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class EmptyDef : ValueDef {
        public static readonly string defId = "empty";
        public override XEDefType valueDefType => XEDefType.dtEmpty;

        public override int? size => 0;

        public EmptyDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string GetValue(ValueElement element) {
            return "";
        }

        public override void SetValue(ValueElement element, string value) {}

        public override dynamic DefaultData() {
            return null;
        }

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
            return null;
        }

        public override string GetSortKey(Element element) {
            return "";
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) { }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            if (!isBase) return src;
            src.Add("type", defId);
            return src;
        }
    }
}
