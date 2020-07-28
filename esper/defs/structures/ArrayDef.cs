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

        public ArrayDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "element");
            elementDef = manager.BuildDef(src.Value<JObject>("element"), this);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new ArrayElement(container, this, true);
            // TODO: handle prefixed sizes and stuff
            source.ReadMultiple((uint)dataSize, () => {
                elementDef.ReadElement(container, source);
            });
            return e;
        }

        public override Element InitElement(Container container) {
            return new ArrayElement(container, this);
        }
    }
}
