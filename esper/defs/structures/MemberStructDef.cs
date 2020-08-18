using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class MemberStructDef : MembersDef {
        public static string defType = "memberStruct";

        private readonly List<int> sortKeyIndices;

        public MemberStructDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            sortKeyIndices = JsonHelpers.List<int>(src, "sortKey");
        }

        public override Element PrepareElement(Container container) {
            return new MemberStructElement(container, this);
        }

        internal bool HandleSubrecord(
            Container container, PluginFileSource source, ref int defIndex
        ) {
            var subrecord = source.currentSubrecord;
            var def = GetMemberDef(subrecord.signature, ref defIndex);
            if (def == null) return false;
            if (def.IsSubrecord()) {
                def.ReadElement(container, source, subrecord.dataSize);
                source.SubrecordHandled();
            } else {
                var e = (Container)def.PrepareElement(container);
                def.SubrecordFound(e, source);
            }
            defIndex++;
            return true;
        }

        public override void SubrecordFound(
            Container container, PluginFileSource source
        ) {
            int defIndex = 0;
            while (defIndex < memberDefs.Count) {
                bool handled = HandleSubrecord(container, source, ref defIndex);
                if (!handled) break;
                if (!source.NextSubrecord()) break;
            }
        }

        public void InitChildElements(Container container) {
            foreach (var memberDef in memberDefs) {
                if (!memberDef.required) continue;
                var e = memberDef.NewElement(container);
                e.Initialize();
            }
        }

        public override string GetSortKey(Element element) {
            return ElementHelpers.StructSortKey(element, sortKeyIndices);
        }
    }
}
