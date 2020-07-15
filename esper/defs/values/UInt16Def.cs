using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs.values {
    public class UInt16Def : ValueDef {
        public static readonly string defType = "uint16";
        public new int size { get => 2; }

        public UInt16Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public new DataContainer ReadData(PluginFileSource source) {
            UInt16 data = source.reader.ReadUInt16();
            return new IntData<UInt16>(data);
        }

        public new DataContainer DefaultData() {
            return new IntData<UInt16>(0);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new IntData<UInt16>(UInt16.Parse(value));
        }
    }
}
