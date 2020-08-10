using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class UnionDef : MaybeSubrecordDef {
        public static string defType = "union";
        public ReadOnlyCollection<ElementDef> elementDefs;
        private readonly Decider decider;

        public UnionDef(DefinitionManager manager, JObject src, Def parent) 
            : base(manager, src, parent) {
            elementDefs = JsonHelpers.ElementDefs(src, "elements", this);
            decider = JsonHelpers.Decider(src, this);
        }

        public ElementDef ResolveDef(Container container) {
            var index = decider.Decide(container);
            return elementDefs[index];
        }

        public override Element ReadElement(
            Container container, PluginFileSource source, UInt16? dataSize = null
        ) {
            var resolvedDef = ResolveDef(container);
            if (resolvedDef is ValueDef valueDef) {
                return new UnionValueElement(container, valueDef, true) {
                    _data = valueDef.ReadData(source, dataSize)
                };
            } else {
                var e = new UnionElement(container, this, true);
                resolvedDef.ReadElement(e, source);
                return e;
            }
        }

        public override Element InitElement(Container container) {
            var resolvedDef = ResolveDef(container);
            if (resolvedDef is ValueDef)
                return new UnionValueElement(container, this);
            return new UnionElement(container, this);
        }
    }
}
