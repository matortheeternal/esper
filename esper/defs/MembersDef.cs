using esper.data;
using esper.helpers;
using esper.setup;

namespace esper.defs {
    //[JSExport]
    public class MembersDef : ElementDef {
        private readonly bool _canContainFormIds;

        public List<ElementDef> memberDefs;
        public HashSet<Signature> signatures;
        private readonly Dictionary<int, ElementDef> unknownMemberDefs =
            new Dictionary<int, ElementDef>();
        public override bool canContainFormIds => _canContainFormIds;
        public override List<ElementDef> childDefs => memberDefs;

        internal MembersDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            memberDefs = JsonHelpers.Defs<ElementDef>(manager, src, "members");
            signatures = GetSignatures();
            _canContainFormIds = memberDefs.Any(d => d.canContainFormIds);
        }

        public ElementDef GetMemberDef(Signature signature, ref int defIndex) {
            if (!signatures.Contains(signature)) return null;
            for (; defIndex < memberDefs.Count; defIndex++) {
                var memberDef = memberDefs[defIndex];
                if (memberDef.ContainsSignature(signature)) return memberDef;
            }
            return null;
        }

        // Builds (and caches) a variable-size byte-array def for a member
        // signature with no definition, so unknown subrecords still decode
        // as elements named "Unknown" and round-trip byte-identical instead
        // of being sidelined as unexpected subrecords (which are dropped
        // on the element-write path). Deliberately kept out of memberDefs
        // and signatures so export, ordering, and lookup semantics stay
        // driven by real definitions only.
        internal ElementDef GetUnknownMemberDef(
            Signature signature, string context
        ) {
            if (!unknownMemberDefs.TryGetValue(signature.v, out var def)) {
                var src = new JObject {
                    { "type", BytesDef.defId },
                    { "name", "Unknown" },
                    { "signature", signature.ToString() }
                };
                def = new BytesDef(manager, src);
                unknownMemberDefs[signature.v] = def;
                manager.ReportDefinitionGap(
                    $"Unknown member {signature} in {context}");
            }
            return def;
        }

        public override bool ContainsSignature(Signature signature) {
            return signatures.Contains(signature);
        }

        public override HashSet<Signature> GetSignatures(HashSet<Signature> sigs = null) {
            if (sigs == null) sigs = new HashSet<Signature>();
            foreach (var def in memberDefs)
                def.GetSignatures(sigs);
            return sigs;
        }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            if (!isBase) return src;
            src.Add("members", src["children"]);
            src.Remove("children");
            return src;
        }
    }
}
