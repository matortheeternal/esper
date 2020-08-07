using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ElementCounter : CounterDef {
        public static string defType = "elementCounter";
        public override bool canSetCount => true;

        public virtual string path => src.Value<string>("path");

        public ElementCounter(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "path");
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
