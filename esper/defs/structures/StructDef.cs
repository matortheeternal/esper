using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.defs {
    public class StructDef : MaybeSubrecordDef {
        public readonly static string defType = "struct";

        public ReadOnlyCollection<ElementDef> elementDefs;
        private readonly List<int> sortKeyIndices;

        public override int? size => elementDefs.Sum(def => def.size);

        public StructDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            elementDefs = JsonHelpers.ElementDefs(src, "elements", this);
            sortKeyIndices = JsonHelpers.List<int>(src, "sortKey");
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new StructElement(container, this);
            ReadChildElements(e, source, dataSize);
            return e;
        }

        public override Element NewElement(Container container) {
            return new StructElement(container, this);
        }

        public void InitChildElements(StructElement element) {
            foreach (var def in elementDefs) {
                var e = def.NewElement(element);
                e.Initialize();
            }
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

        public override string GetSortKey(Element element) {
            return ElementHelpers.StructSortKey(element, sortKeyIndices);
        }
    }
}
