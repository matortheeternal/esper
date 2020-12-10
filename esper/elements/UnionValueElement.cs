using esper.defs;

namespace esper.elements {
    public class UnionValueElement : ValueElement {
        public UnionDef unionDef;

        public override string signature => unionDef.signature;
        public override string name => unionDef.name;
        public override string displayName => signature != null
            ? $"{signature} - {name}"
            : name;

        public UnionValueElement(
            Container container, ElementDef def, UnionDef unionDef
        ) : base(container, def) {
            this.unionDef = unionDef;
        }

        internal override Element CopyInto(Container container, CopyOptions options) {
            return new UnionValueElement(container, def, unionDef) {
                _data = this._data
            };
        }
    }
}
