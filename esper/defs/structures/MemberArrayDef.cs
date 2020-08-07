using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace esper.defs {
    public class MemberArrayDef : ElementDef {
        public static string defType = "memberArray";

        public ElementDef memberDef;
        public CounterDef counterDef;

        public MemberArrayDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "member");
            memberDef = (ElementDef) manager.BuildDef(src.Value<JObject>("member"), this);
            if (!src.ContainsKey("counter")) return;
            counterDef = (CounterDef) manager.BuildDef(src.Value<JObject>("counter"), this);
        }

        public override bool ContainsSignature(string signature) {
            return memberDef.ContainsSignature(signature);
        }

        public override Element PrepareElement(Container container) {
            return container.FindElementForDef(this) ??
                new MemberArrayElement(container, this, true);
        }

        public override void SubrecordFound(
            Container container, PluginFileSource source, string sig, UInt16 size
        ) {
            if (memberDef.IsSubrecord()) {
                memberDef.ReadElement(container, source, size);
            } else {
                var e = container.elements.LastOrDefault();
                if (e == null || e.HasSubrecord(sig))
                    e = memberDef.PrepareElement(container);
                memberDef.SubrecordFound(e as Container, source, sig, size);
            }
        }

        public override Element InitElement(Container container) {
            return new MemberArrayElement(container, this);
        }
    }
}
