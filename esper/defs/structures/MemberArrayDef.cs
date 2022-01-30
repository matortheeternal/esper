using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using esper.data;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class MemberArrayDef : ElementDef {
        public static readonly string defId = "memberArray";
        public override XEDefType defType => XEDefType.dtSubRecordArray;
        public bool isStructArray => memberDef.smashType == SmashType.stStruct;
        public override SmashType smashType => sorted
            ? (isStructArray
                ? SmashType.stSortedStructArray
                : SmashType.stSortedArray)
            : (isStructArray
                ? SmashType.stUnsortedStructArray
                : SmashType.stUnsortedArray);

        public readonly ElementDef memberDef;
        public readonly CounterDef counterDef;
        public readonly bool sorted;

        public override bool canContainFormIds => memberDef.canContainFormIds;
        public override ReadOnlyCollection<ElementDef> childDefs {
            get => new List<ElementDef>() { memberDef }.AsReadOnly();
        }

        public MemberArrayDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            memberDef = JsonHelpers.ElementDef(manager, src, "member");
            counterDef = (CounterDef)JsonHelpers.Def(manager, src, "counter");
            sorted = src.Value<bool>("sorted");
        }

        public override bool ContainsSignature(Signature signature) {
            return memberDef.ContainsSignature(signature);
        }

        public override bool CanEnterWith(Signature signature) {
            return ContainsSignature(signature);
        }

        public override Element PrepareElement(Container container) {
            return container.FindElementForDef(this) ??
                new MemberArrayElement(container, this);
        }

        internal bool HandleSubrecord(Container container, PluginFileSource source) {
            var subrecord = source.currentSubrecord;
            if (!memberDef.CanEnterWith(subrecord.signature)) return false;
            if (memberDef.IsSubrecord()) {
                memberDef.ReadElement(container, source, subrecord.dataSize);
                source.SubrecordHandled();
            } else {
                var e = memberDef.PrepareElement(container);
                memberDef.SubrecordFound(e as Container, source);
            }
            return true;
        }

        public override void SubrecordFound(
            Container container, PluginFileSource source
        ) {
            while (true) {
                bool handled = HandleSubrecord(container, source);
                if (!handled) break;
                if (!source.NextSubrecord()) break;
            }
        }

        public override Element NewElement(Container container = null) {
            return new MemberArrayElement(container, this);
        }

        public override HashSet<Signature> GetSignatures(HashSet<Signature> sigs = null) {
            return memberDef.GetSignatures(sigs);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            output.WriteContainer((Container)element);
        }

        internal void ElementRemoved(Container container) {
            if (counterDef == null || !counterDef.canSetCount) return;
            uint count = (uint)container.internalElements.Count;
            counterDef.SetCount(container, count);
        }
    }
}
