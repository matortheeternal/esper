using esper.data;
using esper.elements;
using esper.setup;

namespace esper.defs {
    public class UnknownGroupDef : GroupDef {
        public static int defGroupType = 10;
        public override int groupType => 10;

        public override bool hasRecordParent => true;
        public override bool isChildGroup => true;

        public override string name => "Unknown";

        public UnknownGroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ConvertLabel(GroupRecord group, byte[] label) {
            return FormId.FromSource(
                group.file,
                BitConverter.ToUInt32(label)
            );
        }

        internal override byte[] ParseLabel(Container container, string name) {
            if (container is GroupRecord group)
                return group.label;
            throw new Exception("Could not determine new group label.");
        }
    }
}
