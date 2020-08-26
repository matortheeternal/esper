using Newtonsoft.Json.Linq;
using esper.helpers;
using esper.setup;
using esper.elements;
using esper.plugins;
using System;

namespace esper.defs {
    public class ArrayDef : MaybeSubrecordDef {
        public static readonly string defType = "array";

        public readonly ElementDef elementDef;
        public readonly CounterDef counterDef;
        public readonly uint? count;
        public readonly int? prefix;
        public readonly int? padding;
        public readonly bool sorted;

        public override bool canContainFormIds => elementDef.canContainFormIds;

        public ArrayDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            elementDef = JsonHelpers.ElementDef(manager, src, "element");
            counterDef = (CounterDef)JsonHelpers.Def(manager, src, "counter");
            count = src.Value<uint?>("count");
            prefix = src.Value<int?>("prefix");
            padding = src.Value<int?>("padding");
            sorted = src.Value<bool>("sorted");
        }

        public UInt32? GetCount(Container container, PluginFileSource source) {
            return count ?? 
                source.ReadPrefix(prefix, padding) ?? 
                counterDef?.GetCount(container);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new ArrayElement(container, this);
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

        public override Element NewElement(Container container) {
            return new ArrayElement(container, this);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            base.WriteElement(element, output);
            throw new NotImplementedException();
        }
    }
}
