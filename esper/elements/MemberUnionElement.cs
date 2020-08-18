using esper.defs;
using System.Linq;

namespace esper.elements {
    public class MemberUnionElement : Container {
        public MemberUnionDef unionDef => def as MemberUnionDef;

        public MemberUnionElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {}

        public override void Initialize() {
            var e = unionDef.defaultDef.NewElement(this);
            e.Initialize();
        }

        public override bool SupportsSignature(string sig) {
            return unionDef.memberDefs.Any(d => d.HasSignature(sig));
        }
    }
}
