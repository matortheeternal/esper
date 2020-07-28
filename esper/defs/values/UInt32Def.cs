using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class UInt32Def : ValueDef {
        public static readonly string defType = "uint32";
        public new int size { get => 4; }

        public UInt32Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? size) {
            return source.reader.ReadUInt32();
        }

        public override dynamic DefaultData() {
            return 0;
        }

        public override string GetValue(ValueElement element) {
            UInt32 data = element.data;
            return data.ToString();
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = UInt32.Parse(value);
        }
    }
}