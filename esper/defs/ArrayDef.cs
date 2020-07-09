using Newtonsoft.Json.Linq;
using esper.helpers;

namespace esper.defs {
    public class ArrayDef : MaybeSubrecordDef {
        public Def elementDef;

        public ArrayDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "element");
            elementDef = manager.BuildDef(src.Value<JObject>("element"));
        }
    }
}
