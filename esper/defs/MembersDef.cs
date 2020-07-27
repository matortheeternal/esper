using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class MembersDef : Def {
        public ReadOnlyCollection<Def> memberDefs;

        public MembersDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "members");
            memberDefs = manager.BuildDefs(src.Value<JArray>("members"), this);
        }

        public Def GetMemberDef(string signature) {
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
