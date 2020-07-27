using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class MemberStructDef : MembersDef {
        public static string defType = "memberStruct";

        public MemberStructDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override Element PrepareElement(Container container) {
            return container.FindElementForDef(this) ??
                new MemberStructElement(container, this, true);
        }

        public override void ReadSubrecord(
            Container container, PluginFileSource source, Signature sig, UInt16 size
        ) {
            // TODO
        }

        public void InitChildElements(Container container) {
            // TODO
        }
    }
}
