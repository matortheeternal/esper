using Newtonsoft.Json.Linq;
using esper.helpers;
using esper.setup;

namespace esper.defs {
    public class ArrayDef : MaybeSubrecordDef {
        public Def elementDef;

        public ArrayDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "element");
            elementDef = manager.BuildDef(src.Value<JObject>("element"), this);
        }
    }
}
