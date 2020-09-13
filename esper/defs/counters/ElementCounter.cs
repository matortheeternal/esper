using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ElementCounter : CounterDef {
        public static readonly string defId = "elementCounter";

        private readonly string _path;

        public override bool canSetCount => true;
        public virtual string path => _path;

        public ElementCounter(DefinitionManager manager, JObject src)
            : base(manager, src) {
            _path = src.Value<string>("path");
            if (path == null) throw new Exception("Path property is null.");
        }

        public override void SetCount(Container container, UInt32 count) {
            // TODO: change to AddElement
            var e = (ValueElement) container.GetElement(path);
            // TODO: error
            if (e == null) return;
            e.data = count;
        }

        public override UInt32 GetCount(Container container) {
            var e = (ValueElement)container.GetElement(path);
            return (UInt32) (e == null ? 0 : e.data);
        }
    }
}
