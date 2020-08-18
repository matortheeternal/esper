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

        public UnionDef(DefinitionManager manager, JObject src) 
            : base(manager, src) {
            elementDefs = JsonHelpers.Defs<ElementDef>(manager, src, "elements");
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
            if (!IsSubrecord() && resolvedDef is ValueDef valueDef) {
                return new UnionValueElement(container, valueDef, this) {
                    _data = valueDef.ReadData(source, dataSize)
                };
            } else {
                var e = new UnionElement(container, this);
                resolvedDef.ReadElement(e, source);
                return e;
            }
        }

        public override Element NewElement(Container container) {
            var resolvedDef = ResolveDef(container);
            if (resolvedDef is ValueDef)
                return new UnionValueElement(container, resolvedDef, this);
            return new UnionElement(container, this);
        }
    }
}
