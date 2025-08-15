using esper.io;
using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class Int0Def : ValueDef {
        public static readonly string defId = "int0";
        public override XEDefType valueDefType => XEDefType.dtInteger;
        public override bool isNumeric => true;

        public override int? size => 0;

        public Int0Def(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
            return null;
        }

        public override dynamic DefaultData() {
            return null;
        }

        public override string GetValue(ValueElement element) {
            return "";
        }

        public override void SetValue(ValueElement element, string value) {
            element._data = null;
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