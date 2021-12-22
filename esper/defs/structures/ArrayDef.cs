using Newtonsoft.Json.Linq;
using esper.helpers;
using esper.setup;
using esper.elements;
using esper.plugins;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace esper.defs {
    public class ArrayDef : MaybeSubrecordDef {
        public static readonly string defId = "array";
        public override XEDefType defType => XEDefType.dtArray;
        public bool isStructArray => elementDef.smashType == SmashType.stStruct;
        public override SmashType smashType => sorted
            ? (isStructArray
                ? SmashType.stSortedStructArray
                : SmashType.stSortedArray)
            : (isStructArray
                ? SmashType.stUnsortedStructArray
                : SmashType.stUnsortedArray);

        public readonly ElementDef elementDef;
        public readonly CounterDef counterDef;
        public readonly uint? count;
        public readonly int? prefix;
        public readonly int? padding;
        public readonly bool sorted;

        public override bool canContainFormIds => elementDef.canContainFormIds;
        public override ReadOnlyCollection<ElementDef> childDefs {
            get => new List<ElementDef>() { elementDef }.AsReadOnly();
        }

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
            Container container, PluginFileSource source, UInt32? dataSize = null
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

        public override Element NewElement(Container container = null) {
            return new ArrayElement(container, this);
        }

        internal override UInt32 GetSize(Element element) {
            UInt32 size = 0;
            if (prefix != null) size += (UInt32) prefix;
            if (padding != null) size += (UInt32) padding;
            return size + base.GetSize(element);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            base.WriteElement(element, output);
            var container = (Container) element;
            if (prefix != null) 
                output.WritePrefix(container.count, (int) prefix, padding ?? 0);
            output.WriteContainer((Container) element);
        }

        internal void ElementRemoved(Container container) {
            if (counterDef == null || !counterDef.canSetCount) return;
            uint count = (uint)container.internalElements.Count;
            counterDef.SetCount(container, count);
        }
    }
}
