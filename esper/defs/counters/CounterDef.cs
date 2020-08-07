using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class CounterDef : Def {
        public virtual bool canSetCount => false;

        public CounterDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public virtual void SetCount(Container container, UInt32 count) {
            throw new NotImplementedException();
        }

        public virtual UInt32 GetCount(Container container) {
            throw new NotImplementedException();
        }
    }
}
