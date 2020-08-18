using esper.elements;
using esper.setup;
using esper.plugins;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace esper.defs {
    public class MemberUnionDef : MembersDef {
        public static string defType = "memberUnion";

        public ElementDef defaultDef => memberDefs[0];

        public MemberUnionDef(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        public override Element PrepareElement(Container container) {
            if (defaultDef.IsSubrecord()) return container;
            return container.FindElementForDef(this) ??
                new MemberUnionElement(container, this, true);
        }

        public override void SubrecordFound(
            Container container, PluginFileSource source, string sig, UInt16 size
        ) {
            var memberDef = GetMemberDef(sig);
            if (memberDef.IsSubrecord()) {
                memberDef.ReadElement(container, source, size);
            } else {
                memberDef.SubrecordFound(container, source, sig, size);
            }
        }

        public override Element NewElement(Container container) {
            var defaultDef = this.defaultDef;
            if (defaultDef.IsSubrecord())
                return defaultDef.NewElement(container);
            return new MemberUnionElement(container, this);
        }

        public override bool HasSignature(string sig) {
            return defaultDef.IsSubrecord() && 
                memberDefs.Any(d => d.HasSignature(sig));
        }
    }
}
