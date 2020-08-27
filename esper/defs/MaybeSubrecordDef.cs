using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
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

        public override bool CanEnterWith(string signature) {
            return this.signature == signature;
        }

        public override List<string> GetSignatures(List<string> sigs = null) {
            if (sigs == null) sigs = new List<string>();
            if (signature != null) sigs.Add(signature);
            return sigs;
        }

        internal override UInt32 GetSize(Element element) {
            return (UInt32) ((IsSubrecord() ? 6 : 0) + base.GetSize(element));
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            if (!IsSubrecord()) return;
            output.WriteString(_signature);
            output.writer.Write((UInt16) (element.def.GetSize(element) - 6));
        }
    }
}
