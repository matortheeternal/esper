using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class MemberUnionDef : MembersDef {
        public static string defType = "memberUnion";
        private readonly Decider decider;

        public MemberUnionDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "decider");
            decider = manager.GetDecider(src.Value<string>("decider"));
        }

        public override Element PrepareElement(Container container) {
            return container.FindElementForDef(this) ??
                new MemberUnionElement(container, this, true);
        }

        public Def ResolveDef(Container container) {
            var index = decider.Decide(container);
            return memberDefs[index];
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var resolvedDef = ResolveDef(container);
            if (resolvedDef.IsSubrecord()) {
                return resolvedDef.ReadElement(container, source, dataSize);
            } else {
                var e = new MemberUnionElement(container, this, true);
                resolvedDef.ReadElement(e, source);
                return e;
            }
        }

        public override Element InitElement(Container container) {
            var resolvedDef = ResolveDef(container);
            if (resolvedDef.IsSubrecord())
                return resolvedDef.InitElement(container);
            return new MemberUnionElement(container, this);
        }
    }
}
