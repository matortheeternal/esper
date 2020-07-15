using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class StructDef : MaybeSubrecordDef {
        public static string defType = "struct";
        public List<Def> elementDefs;

        public StructDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "elements");
            elementDefs = manager.BuildDefs(src.Value<JArray>("elements"), this);
        }

        public void Read(StructElement element, PluginFileSource source) {
            elementDefs.ForEach(def => def.ReadElement(element, source));
        } 
    }
}
