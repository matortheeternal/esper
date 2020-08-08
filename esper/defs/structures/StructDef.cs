using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.defs {
    public class StructDef : MaybeSubrecordDef {
        public readonly static string defType = "struct";
        public ReadOnlyCollection<ElementDef> elementDefs;
        public override int? size => elementDefs.Sum(def => def.size);

        public StructDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            elementDefs = JsonHelpers.ElementDefs(src, "elements", this);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new StructElement(container, this, true);
            ReadChildElements(e, source, dataSize);
            return e;
        }

        public override Element InitElement(Container container) {
            return new StructElement(container, this);
        }

        public void InitChildElements(StructElement element) {
            foreach (var def in elementDefs)
                def.InitElement(element);
        }

        public void ReadChildElements(
            StructElement element, PluginFileSource source, UInt16? dataSize
        ) {
            var remainingSize = dataSize;
            var pos = source.stream.Position;
            foreach (var def in elementDefs) {
                var e = def.ReadElement(element, source, remainingSize);
                if (remainingSize == 0) return; // early struct termination
                if (dataSize != null) {
                    var newPos = source.stream.Position;
                    var diff = newPos - pos;
                    pos = newPos;
                    remainingSize -= (UInt16) diff;
                }
            }
        }
    }
}
