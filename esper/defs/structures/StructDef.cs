using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.defs {
    public class StructDef : MaybeSubrecordDef {
        public readonly static string defId = "struct";
        public override XEDefType defType => XEDefType.dtStruct;
        public override SmashType smashType => SmashType.stStruct;

        private readonly bool _canContainFormIds;
        internal ReadOnlyCollection<ElementDef> elementDefs;
        internal readonly List<int> sortKeyIndices;
        internal readonly List<int> elementMap;

        public override int? size => elementDefs.Sum(def => def.size);
        public override bool canContainFormIds => _canContainFormIds;
        public override ReadOnlyCollection<ElementDef> childDefs => elementDefs;

        public StructDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            elementDefs = JsonHelpers.Defs<ElementDef>(manager, src, "elements");
            sortKeyIndices = JsonHelpers.List<int>(src, "sortKey");
            elementMap = JsonHelpers.List<int>(src, "elementMap");
            _canContainFormIds = elementDefs.Any(d => d.canContainFormIds);
        }

        public StructDef(StructDef other) : base(other) {
            elementDefs = new ReadOnlyCollection<ElementDef>(other.elementDefs);
            sortKeyIndices = other.sortKeyIndices == null 
                ? null
                : new List<int>(other.sortKeyIndices);
            elementMap = other.elementMap == null
                ? null
                : new List<int>(other.elementMap);
            _canContainFormIds = other._canContainFormIds;
        }

        public override Element ReadElement(
            Container container, DataSource source, UInt32? dataSize = null
        ) {
            var e = new StructElement(container, this);
            ReadChildElements(e, source, dataSize);
            return e;
        }

        public override Element NewElement(Container container = null) {
            return new StructElement(container, this);
        }

        public void InitChildElements(StructElement element) {
            foreach (var def in elementDefs) {
                var e = def.NewElement(element);
                e.Initialize();
            }
        }

        private UInt32? GetRemainingSize(
            DataSource source, long startPos, UInt32? dataSize
        ) {
            if (dataSize == null) return null;
            return (UInt32?)(dataSize - (source.stream.Position - startPos));
        }

        // TODO: rewrite this better or make it unnecessary?
        public void ReadChildElements(
            StructElement element, DataSource source, UInt32? dataSize
        ) {
            var startPos = source.stream.Position;
            var lastDefIndex = elementDefs.Count - 1;
            for (int i = 0; i <= lastDefIndex; i++) {
                var def = elementDefs[i];
                if (source.stream.Position >= source.dataEndPos) {
                    def.NewElement(element);
                } else {
                    UInt32? remainingSize = (i == lastDefIndex)
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
            output.WriteContainer((Container)element);
        }

        internal int GetInternalOrder(ElementDef childDef) {
            var index = elementDefs.IndexOf(childDef);
            if (index == -1) 
                throw new Exception($"Element {childDef.name} is not supported.");
            return elementMap != null
                ? elementMap[index]
                : index;
        }
    }
}
