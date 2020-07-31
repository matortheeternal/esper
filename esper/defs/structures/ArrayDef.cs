using Newtonsoft.Json.Linq;
using esper.helpers;
using esper.setup;
using esper.elements;
using esper.parsing;
using System;

namespace esper.defs {
    public class ArrayDef : MaybeSubrecordDef {
        public static string defType = "array";
        public Def elementDef;
        public int? prefix => src.Value<int?>("prefix");
        private int? padding => src.Value<int?>("padding");

        public ArrayDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "element");
            elementDef = manager.BuildDef(src.Value<JObject>("element"), this);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new ArrayElement(container, this, true);
            UInt32? count = source.ReadPrefix(prefix, padding);
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
