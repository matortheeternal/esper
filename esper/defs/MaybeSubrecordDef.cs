using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class MaybeSubrecordDef : ElementDef {
        private readonly string _signature;

        public override string signature => _signature;
        public override string displayName => signature != null
            ? $"{signature} - {name}"
            : name;

        public MaybeSubrecordDef(DefinitionManager manager, JObject src) 
            : base(manager, src) {
            _signature = src.Value<string>("signature");
        }

        public override bool ContainsSignature(string signature) {
            return this.signature == signature;
        }

        public override List<string> GetSignatures(List<string> sigs = null) {
            if (sigs == null) sigs = new List<string>();
            if (signature != null) sigs.Add(signature);
            return sigs;
        }
    }
}
