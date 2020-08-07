using Newtonsoft.Json.Linq;
using esper.helpers;
using esper.setup;
using esper.elements;
using esper.parsing;
using System;

namespace esper.defs {
    public class ArrayDef : MaybeSubrecordDef {
        public static string defType = "array";

        public ElementDef elementDef;
        public CounterDef counterDef;

        public uint? count => src.Value<uint?>("count");
        public int? prefix => src.Value<int?>("prefix");
        private int? padding => src.Value<int?>("padding");

        public ArrayDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "element");
            elementDef = (ElementDef) manager.BuildDef(src.Value<JObject>("element"), this);
            if (!src.ContainsKey("counter")) return;
            counterDef = (CounterDef)manager.BuildDef(src.Value<JObject>("counter"), this);
        }

        public UInt32? GetCount(Container container, PluginFileSource source) {
            return count ?? 
                source.ReadPrefix(prefix, padding) ?? 
                counterDef?.GetCount(container);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new ArrayElement(container, this, true);
            UInt32? count = GetCount(container, source);
            void readChild() => elementDef.ReadElement(e, source);
            if (count != null) {
                for (int i = 0; i < count; i++) readChild();
            } else if (dataSize != null) {
                source.ReadMultiple((UInt32)dataSize, readChild);
            } else {
                throw new Exception("Unknown array size/length.");
            }
            return e;
        }

        public override Element InitElement(Container container) {
            return new ArrayElement(container, this);
        }
    }
}
