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

        private UInt16? GetRemainingSize(
            PluginFileSource source, long startPos, UInt16? dataSize
        ) {
            if (dataSize == null) return null;
            return (UInt16?)(dataSize - (source.stream.Position - startPos));
        }

        // TODO: rewrite this better or make it unnecessary?
        public void ReadChildElements(
            StructElement element, PluginFileSource source, UInt16? dataSize
        ) {
            var startPos = source.stream.Position;
            var lastDefIndex = elementDefs.Count - 1;
            for (int i = 0; i <= lastDefIndex; i++) {
                var def = elementDefs[i];
                if (source.stream.Position - startPos >= dataSize) {
                    def.NewElement(element);
                } else {
                    UInt16? remainingSize = (i == lastDefIndex)
                        ? GetRemainingSize(source, startPos, dataSize)
                        : null;
                    def.ReadElement(element, source, remainingSize);
                }
            }
        }

        public override string GetSortKey(Element element) {
            return ElementHelpers.StructSortKey(element, sortKeyIndices);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            base.WriteElement(element, output);
            throw new NotImplementedException();
        }
    }
}
