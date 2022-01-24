using esper.data;
using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class TopGroupDef : GroupDef {
        private readonly Signature _signature;

        public static int defGroupType = 0;
        public override int groupType => 0;
        public override Signature signature => _signature;
        public override string name => recordDef.name;
        public override string displayName => $"{signature} - {name}";
        public override bool isTopGroup => true;

        public MainRecordDef recordDef { get; }

        public TopGroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            var sig = src.Value<string>("signature");
            _signature = Signature.FromString(sig);
            recordDef = manager.GetRecordDef(signature.ToString());
            if (childrenDefs != null) return;
            childrenDefs = new List<ElementDef>(1) { recordDef }.AsReadOnly();
        }

        public override dynamic ConvertLabel(GroupRecord group, byte[] label) {
            return new Signature(label);
        }

        internal override byte[] ParseLabel(Container container, string name) {
            return signature.bytes;
        }
    }
}
