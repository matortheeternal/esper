using esper.data;
using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class ChildGroupDef : GroupDef {
        public override bool hasRecordParent => true;
        public override bool isChildGroup => true;

        public ChildGroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ConvertLabel(GroupRecord group, byte[] label) {
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

    public class WorldChildrenDef : ChildGroupDef {
        public static int defGroupType = 1;
        public override int groupType => 1;

        public WorldChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class CellChildrenDef : ChildGroupDef {
        public static int defGroupType = 6;
        public override int groupType => 6;

        public CellChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class TopicChildrenDef : ChildGroupDef {
        public static int defGroupType = 7;
        public override int groupType => 7;

        public TopicChildrenDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
