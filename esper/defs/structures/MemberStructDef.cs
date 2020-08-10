using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace esper.defs {
    public class MemberStructDef : MembersDef {
        public static string defType = "memberStruct";

        private readonly List<int> sortKeyIndices;

        public MemberStructDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            sortKeyIndices = JsonHelpers.List<int>(src, "sortKey");
        }

        public override Element PrepareElement(Container container) {
            return container.FindElementForDef(this) ??
                new MemberStructElement(container, this, true);
        }

        public override void SubrecordFound(
            Container container, PluginFileSource source, string sig, UInt16 size
        ) {
            var memberDef = GetMemberDef(sig);
            if (memberDef.IsSubrecord()) {
                memberDef.ReadElement(container, source, size);
            } else {
                var e = memberDef.PrepareElement(container);
                memberDef.SubrecordFound(e as Container, source, sig, size);
            }
        }

        public void InitChildElements(Container container) {
            foreach (var memberDef in memberDefs)
                if (memberDef.required) memberDef.InitElement(container);
        }

        public override string GetSortKey(Element element) {
            return ElementHelpers.StructSortKey(element, sortKeyIndices);
        }
    }
}
