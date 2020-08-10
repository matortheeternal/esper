using esper.defs;
using System.Linq;

namespace esper.elements {
    public class MemberArrayElement : Container {
        public MemberArrayDef maDef => def as MemberArrayDef;

        public MemberArrayElement(Container container, ElementDef def, bool skipInit = false)
            : base(container, def) {
            if (skipInit) return;
            maDef.memberDef.InitElement(this);
        }

        public override bool SupportsSignature(string sig) {
            return maDef.memberDef.HasSignature(sig);
        }

        internal override void ElementsReady() {
            base.ElementsReady();
            if (!maDef.sorted) return;
            var sortKeys = _elements.ToDictionary(e => e, e => e.sortKey);
            _elements.Sort((e1, e2) => {
                return sortKeys[e1].CompareTo(sortKeys[e2]);
            });
        }
    }
}
