using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class MembersDef : ElementDef {
        public ReadOnlyCollection<ElementDef> memberDefs;

        public MembersDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "members");
            memberDefs = manager.BuildDefs(src.Value<JArray>("members"), this);
        }

        public ElementDef GetMemberDef(string signature) {
            foreach (var def in memberDefs)
                if (def.ContainsSignature(signature)) return def;
            return null;
        }

        public override bool ContainsSignature(string signature) {
            foreach (var def in memberDefs)
                if (def.ContainsSignature(signature)) return true;
            return false;
        }
    }
}
