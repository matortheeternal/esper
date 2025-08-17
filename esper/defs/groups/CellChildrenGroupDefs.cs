using esper.data;
using esper.elements;
using esper.setup;

namespace esper.defs {
    public class CellChildrenGroupDef : GroupDef {
        public override bool hasRecordParent => true;
        public override bool isChildGroup => true;
        public override bool isChildGroupChild => true;

        public CellChildrenGroupDef(DefinitionManager manager, JObject src)
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

    public class CellPersistentChildrenDef : CellChildrenGroupDef {
        public static int defGroupType = 8;
        public override int groupType => 8;

        public override string name => "Persistent";

        public CellPersistentChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class CellTemporaryChildrenDef : CellChildrenGroupDef {
        public static int defGroupType = 9;
        public override int groupType => 9;

        public override string name => "Temporary";

        public CellTemporaryChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
