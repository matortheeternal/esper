using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class MembersDef : ElementDef {
        public ReadOnlyCollection<ElementDef> memberDefs;
        private readonly Dictionary<string, ElementDef> sigDefMap;

        public MembersDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            memberDefs = JsonHelpers.ElementDefs(src, "members", this);
            sigDefMap = new Dictionary<string, ElementDef>();
            foreach (var def in memberDefs)
                def.GetSignatures().ForEach(sig => sigDefMap[sig] = def);
        }

        public ElementDef GetMemberDef(string signature) {
            if (!sigDefMap.ContainsKey(signature)) return null;
            return sigDefMap[signature];
        }

        public override bool ContainsSignature(string signature) {
            return sigDefMap.ContainsKey(signature);
        }

        public override List<string> GetSignatures(List<string> sigs = null) {
            if (sigs == null) sigs = new List<string>();
            foreach (var def in memberDefs)
                def.GetSignatures(sigs);
            return sigs;
        }
    }
}
