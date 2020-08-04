using esper.defs;
using System.Linq;

namespace esper.elements {
    public class MemberUnionElement : Container {
        public MemberUnionDef unionDef => def as MemberUnionDef;

        public MemberUnionElement(Container container, Def def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            unionDef.defaultDef.InitElement(this);
        }

        public override bool SupportsSignature(string sig) {
            return unionDef.memberDefs.Any(d => d.HasSignature(sig));
        }
    }
}
