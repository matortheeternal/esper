using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    // A transparent reference to the array def levelsUp containers above
    // the receiving union/array element. xEdit's TwbRecursiveDef climbs
    // the lexical def-parent chain levelsUp steps starting at defParent,
    // requiring a value def at each step and yielding nil otherwise.
    // esper defs are shared singletons with no parent pointers, but
    // union/array runtime elements materialize 1:1 with their defs, so
    // the receiving element already carries the first node as its .def
    // and levelsUp-1 container climbs reach the same def. In the only
    // real usage (FO4 ScriptPropertyStruct) both the union arms
    // (levelsUp 3) and the array element (levelsUp 4) resolve to the
    // 'Struct' ARRAY def, so read/write delegate to it directly and the
    // runtime element is a plain ArrayElement: despite the name, this is
    // never a struct.
    public class RecursiveDef : ElementDef {
        public static readonly string defId = "recursive";
        public override XEDefType defType => XEDefType.dtStruct;
        public override SmashType smashType => SmashType.stStruct;

        private readonly int _levelsUp;
        internal int levelsUp => _levelsUp;
        public override bool canContainFormIds => true;

        internal RecursiveDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            _levelsUp = src.Value<int>("levelsUp");
            if (_levelsUp <= 0)
                throw new Exception("Recursive def must have levelsUp > 0");
        }

        public ElementDef ResolveDef(Container container) {
            // xEdit yields nil past the root: no container, or a climb
            // through a main record (record defs are not value defs).
            if (container == null || container is MainRecord) return null;
            var target = container;
            for (int i = 1; i < _levelsUp; i++) {
                target = target.container as Container;
                if (target == null || target is MainRecord) return null;
            }
            return target.def;
        }

        private ElementDef ResolvedOrThrow(Container container) {
            return ResolveDef(container) ?? throw new Exception(
                $"Recursive def {name} could not resolve " +
                $"({levelsUp} levels up from {container?.def?.name}).");
        }

        public override Element ReadElement(
            Container container, DataSource source, UInt32? dataSize = null
        ) {
            return ResolvedOrThrow(container)
                .ReadElement(container, source, dataSize);
        }

        public override Element NewElement(Container container = null) {
            return ResolvedOrThrow(container).NewElement(container);
        }

        public override string GetSortKey(Element element) {
            var resolvedDef = ResolveDef((Container)element.container);
            return resolvedDef?.GetSortKey(element) ?? "";
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            ResolvedOrThrow((Container)element.container)
                .WriteElement(element, output);
        }
    }
}
