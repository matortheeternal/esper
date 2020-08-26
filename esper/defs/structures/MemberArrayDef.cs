using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class MemberArrayDef : ElementDef {
        public static readonly string defType = "memberArray";

        public readonly ElementDef memberDef;
        public readonly CounterDef counterDef;
        public readonly bool sorted;

        public override bool canContainFormIds => memberDef.canContainFormIds;

        public MemberArrayDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            memberDef = JsonHelpers.ElementDef(manager, src, "member");
            counterDef = (CounterDef)JsonHelpers.Def(manager, src, "counter");
            sorted = src.Value<bool>("sorted");
        }

        public override bool ContainsSignature(string signature) {
            return memberDef.ContainsSignature(signature);
        }

        public override bool CanEnterWith(string signature) {
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

        public override Element NewElement(Container container) {
            return new MemberArrayElement(container, this);
        }

        public override List<string> GetSignatures(List<string> sigs = null) {
            return memberDef.GetSignatures(sigs);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            output.WriteContainer((Container)element);
        }
    }
}
