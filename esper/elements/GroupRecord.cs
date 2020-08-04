using esper.defs;
using esper.parsing;
using esper.plugins;
using esper.resolution;
using System;
using System.IO;

namespace esper.elements {
    public enum GroupType : Int32 {
        Top = 0,
        WorldChildren = 1,
        InteriorCellBlock = 2,
        InteriorCellSubBlock = 3,
        ExteriorCellBlock = 4,
        ExteriorCellSubBlock = 5,
        CellChildren = 6,
        TopicChildren = 7,
        CellPersistentChildren = 8,
        CellTemporaryChildren = 9,
        NonChildGroups = Top | InteriorCellBlock | InteriorCellSubBlock,
        AllChildGroups = ~NonChildGroups,
        ChildGroupChild = ExteriorCellBlock | ExteriorCellSubBlock
    };

    public class GroupRecord : Container {
        private static readonly Signature GRUP = Signature.FromString("GRUP");

        public readonly StructElement header;

        private StructDef groupHeaderDef => manager.groupHeaderDef as StructDef;
        public override string signature => header.GetValue("Signature");
        public UInt32 groupSize => header.GetData("Group Size");
        public GroupType groupType => (GroupType)header.GetData("Group Type");
        public byte[] label => header.GetData("Label");
        public UInt32 dataSize => (UInt32)(groupSize - groupHeaderDef.size);
        public bool isChildGroup => (groupType & GroupType.AllChildGroups) > 0;
        public bool isChildGroupChild => (groupType & GroupType.ChildGroupChild) > 0;

        private Signature labelAsSignature => new Signature(label);
        private Int32 labelAsInt32 => BitConverter.ToInt32(label);
        private UInt32 labelAsUInt32 => BitConverter.ToUInt32(label);
        private FormId labelAsFormId => FormId.FromSource(file, labelAsUInt32);
        private Int16[] labelAsInt16x2 => new Int16[2] {
            BitConverter.ToInt16(label),
            BitConverter.ToInt16(label, 2)
        };

        public GroupRecord(Container container)
            : base(container) {
            header = (StructElement)groupHeaderDef.InitElement(null);
            header.container = this;
        }

        public GroupRecord(Container container, PluginFileSource source)
            : base(container) {
            header = (StructElement)groupHeaderDef.ReadElement(null, source);
            header.container = this;
            header.SetState(ElementState.Protected);
            if (signature != "GRUP")
                throw new Exception("Expected GRUP record");
        }

        public static GroupRecord Read(Container container, PluginFileSource source) {
            var group = new GroupRecord(container, source);
            source.ReadMultiple(group.dataSize, () => {
                var sig = source.ReadSignature();
                source.stream.Seek(-4, SeekOrigin.Current);
                if (sig == GRUP) {
                    Read(group, source);
                } else {
                    MainRecord.Read(group, source, sig);
                }
            });
            return group;
        }

        public dynamic GetLabel() {
            return groupType switch {
                GroupType.Top => labelAsSignature,
                GroupType.WorldChildren => labelAsFormId,
                GroupType.InteriorCellBlock => labelAsInt32,
                GroupType.InteriorCellSubBlock => labelAsInt32,
                GroupType.ExteriorCellBlock => labelAsInt16x2,
                GroupType.ExteriorCellSubBlock => labelAsInt16x2,
                GroupType.CellChildren => labelAsFormId,
                GroupType.TopicChildren => labelAsFormId,
                GroupType.CellPersistentChildren => labelAsFormId,
                GroupType.CellTemporaryChildren => labelAsFormId,
                _ => throw new Exception("Unknown group type.")
            };
        }

        public MainRecord GetParentRecord() {
            if (isChildGroupChild && container is GroupRecord group)
                return group.GetParentRecord();
            if (!isChildGroup) return null;
            return file.GetRecordByFormId(labelAsUInt32);
        }

        public override bool SupportsSignature(string sig) {
            return groupType switch {
                GroupType.Top => sig == labelAsSignature.ToString(),
                GroupType.WorldChildren => sig == "WRLD",
                GroupType.InteriorCellSubBlock => sig == "CELL",
                GroupType.ExteriorCellSubBlock => sig == "CELL",
                GroupType.TopicChildren => sig == "INFO",
                GroupType.CellChildren => game.CellSupports(sig),
                GroupType.CellPersistentChildren => game.CellSupports(sig),
                GroupType.CellTemporaryChildren => game.CellSupports(sig),
                _ => false
            };
        }
    }
}