using esper.elements;
using esper.resolution;
using esper.setup;

namespace esper.defs {
    public class ElementCounter : CounterDef {
        public static readonly string defId = "elementCounter";

        private readonly string _path;

        public override bool canSetCount => true;
        public virtual string path => _path;

        internal ElementCounter(DefinitionManager manager, JObject src)
            : base(manager, src) {
            _path = src.Value<string>("path");
            if (path == null) throw new Exception("Path property is null.");
        }

        protected ElementCounter(
            DefinitionManager manager, JObject src, string path
        ) : base(manager, src) {
            _path = path ?? throw new Exception("Path property is null.");
        }

        private ValueElement FindCounter(Container container) {
            var counter = container?.GetElement(path) as ValueElement;
            if (counter != null) return counter;
            return (container?.container as Container)?.GetElement(path)
                as ValueElement;
        }

        public override void SetCount(Container container, UInt32 count) {
            var e = FindCounter(container);
            if (e == null) return;
            e.data = count;
        }

        public override UInt32 GetCount(Container container) {
            var e = FindCounter(container);
            return (UInt32)(e == null ? 0 : e.data);
        }
    }
}
