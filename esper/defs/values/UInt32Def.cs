using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs.values {
    public class UInt32Def : ValueDef {
        public static readonly string defType = "uint32";
        public new int size { get => 4; }

        public UInt32Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public new DataContainer ReadData(PluginFileSource source) {
            UInt32 data = source.reader.ReadUInt32();
            return new IntData<UInt32>(data);
        }

        public new DataContainer DefaultData() {
            return new IntData<UInt32>(0);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new IntData<UInt32>(UInt32.Parse(value));
        }
    }
}