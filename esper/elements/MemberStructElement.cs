using esper.defs;
using System.Linq;

namespace esper.elements {
    public class MemberStructElement : Container {
        public MemberStructDef msDef { get => def as MemberStructDef; }

        public MemberStructElement(Container container, ElementDef def)
            : base(container, def) {}

        public override void Initialize() {
            msDef.InitChildElements(this);
        }

        public override bool SupportsSignature(string sig) {
            return msDef.memberDefs.Any(d => d.HasSignature(sig));
        }
    }
}
