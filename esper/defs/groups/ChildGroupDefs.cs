using esper.data;
using esper.elements;
using esper.setup;

namespace esper.defs {
    public class ChildGroupDef : GroupDef {
        public override bool hasRecordParent => true;
        public override bool isChildGroup => true;

        internal ChildGroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override dynamic ConvertLabel(GroupRecord group, byte[] label) {
            return FormId.FromSource(
                group.file,
                BitConverter.ToUInt32(label)
            );
        }

        public override string GetName(GroupRecord group) {
            FormId label = ConvertLabel(group, group.header.label);
            return $"Children of {label.fileFormId:X8}";
        }

        public override bool NameMatches(string name) {
            // TODO: maybe we want to allow this?
            return false;
        }
    }

    //[JSExport]
    public class WorldChildrenDef : ChildGroupDef {
        public static int defGroupType = 1;
        public override int groupType => 1;

        internal WorldChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    //[JSExport]
    public class CellChildrenDef : ChildGroupDef {
        public static int defGroupType = 6;
        public override int groupType => 6;

        internal CellChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    //[JSExport]
    public class TopicChildrenDef : ChildGroupDef {
        public static int defGroupType = 7;
        public override int groupType => 7;

        internal TopicChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    //[JSExport]
    public class QuestChildrenDef : ChildGroupDef {
        public static int defGroupType = 10;
        public override int groupType => 10;

        internal QuestChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
