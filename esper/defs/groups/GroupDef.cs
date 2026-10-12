using esper.data;
using esper.elements;
using esper.helpers;
using esper.io;
using esper.plugins;
using esper.setup;

namespace esper.defs {
    public class GroupDef : ElementDef {
        public static string defId = "group";

        internal List<ElementDef> childrenDefs;

        public override List<ElementDef> childDefs => childrenDefs;
        public override bool canContainFormIds => true;
        public virtual int groupType => throw new NotImplementedException();
        public virtual bool hasRecordParent => false;
        public virtual bool isChildGroup => false;
        public virtual bool isChildGroupChild => false;
        public virtual bool isTopGroup => false;

        internal GroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            FilterDanglingRefs(manager, src);
            childrenDefs = JsonHelpers.Defs<ElementDef>(
                manager, src, "children", true
            );
        }

        // Upstream esp.json data can reference record/group defs that don't
        // exist for a game (e.g. FO3 cellChildGroups list trap records like
        // PFLA that have no record definition). Drop those refs so sessions
        // for such games can still build; fail-loud stays for inline defs.
        private static void FilterDanglingRefs(
            DefinitionManager manager, JObject src
        ) {
            if (src["children"] is not JArray children) return;
            foreach (var child in children.ToList()) {
                if (
                    child is JObject childObj &&
                    childObj.Properties().Count() == 1 &&
                    childObj.ContainsKey("id")
                ) {
                    var id = childObj.Value<string>("id");
                    if (!manager.HasDefEntry(id)) children.Remove(child);
                }
            }
        }

        internal virtual dynamic ConvertLabel(GroupRecord group, byte[] label) {
            throw new NotImplementedException();
        }

        public virtual string GetName(GroupRecord group) {
            return name;
        }

        public string GetDisplayName(GroupRecord group) {
            return displayName ?? GetName(group);
        }

        internal virtual byte[] ParseLabel(Container container, string name) {
            throw new NotImplementedException();
        }

        public GroupRecord CreateFromName(
            Container container, string name
        ) {
            var label = ParseLabel(container, name);
            return new GroupRecord(container, this, label);
        }

        internal void ReadChildren(GroupRecord group, PluginFileSource source) {
            var file = group.file;
            source.ReadMultiple(group.dataSize, () => {
                var sig = source.PeekSignature();
                if (sig == Signatures.GRUP) {
                    GroupRecord.Read(group, source);
                } else {
                    var rec = MainRecord.Read(group, source, sig);
                    file.IndexRecord(rec);
                }
            });
            group.internalElements.TrimExcess();
        }

        internal GroupDef GetChildGroupDef() {
            return childDefs.FirstOrDefault(childDef => {
                return childDef is GroupDef gd && gd.isChildGroup;
            }) as GroupDef;
        }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            src.Add("type", "group");
            src.Remove("name");
            if (groupType != 0) src.Add("groupType", groupType);
            return src;
        }

        internal override bool HasBaseDef() {
            return false;
        }
    }
}
