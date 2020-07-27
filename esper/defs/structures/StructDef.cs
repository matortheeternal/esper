using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class StructDef : MaybeSubrecordDef {
        public readonly static string defType = "struct";
        public ReadOnlyCollection<Def> elementDefs;

        public StructDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            ErrorHelpers.CheckDefProperty(src, "elements");
            elementDefs = manager.BuildDefs(src.Value<JArray>("elements"), this);
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var e = new StructElement(container, this, true);
            ReadChildElements(e, source);
            return e;
        }

        public override Element InitElement(Container container) {
            return new StructElement(container, this);
        }

        public void InitChildElements(StructElement element) {
            foreach (var def in elementDefs)
                def.InitElement(element);
        }

        public void ReadChildElements(StructElement element, PluginFileSource source) {
            foreach (var def in elementDefs)
                def.ReadElement(element, source);
        }
    }
}
