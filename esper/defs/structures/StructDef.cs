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
        private readonly List<int> elementMap;

        public override int? size => elementDefs.Sum(def => def.size);

        public StructDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            elementDefs = JsonHelpers.Defs<ElementDef>(manager, src, "elements");
            sortKeyIndices = JsonHelpers.List<int>(src, "sortKey");
            elementMap = JsonHelpers.List<int>(src, "elementMap");
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
            var startPos = source.stream.Position;
            foreach (var def in elementDefs) {
                if (source.stream.Position - startPos >= dataSize) {
                    def.NewElement(element);
                } else {
                    def.ReadElement(element, source);
                }
            }
        }

        public override string GetSortKey(Element element) {
            return ElementHelpers.StructSortKey(element, sortKeyIndices);
        }

        internal void RemapElements(StructElement se) {
            if (elementMap == null) return;
            se._elements = elementMap.Select(index => {
                return se._elements[index];
            }).ToList();
        }
    }
}
