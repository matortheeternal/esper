using esper.defs;
using esper.data;
using esper.plugins;
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
        CellTemporaryChildren = 9
    };

    public class GroupRecord : Container {
        private static readonly Signature GRUP = Signature.FromString("GRUP");

        public readonly TES4GroupHeader header;
        public EditorIdMap recordsByEditorID;
        private MainRecord _parentRecord;

        public override string name => GetName();
        public override string displayName => name;
        public override string signature => header.signature.ToString();
        public override GroupRecord group => this;

        public UInt32 groupSize => header.groupSize;
        public GroupType groupType => (GroupType)header.groupType;
        public byte[] label => header.label;
        public UInt32 dataSize => (UInt32)(groupSize - groupHeaderDef.size);

        private StructDef groupHeaderDef => manager.groupHeaderDef as StructDef;
        private Signature labelAsSignature => new Signature(label);
        private Int32 labelAsInt32 => BitConverter.ToInt32(label);
        private UInt32 labelAsUInt32 => BitConverter.ToUInt32(label);
        private FormId labelAsFormId => FormId.FromSource(file, labelAsUInt32);
        private Int16[] labelAsInt16x2 => new Int16[2] {
            BitConverter.ToInt16(label),
            BitConverter.ToInt16(label, 2)
        };
        private string coordinatesStr => string.Join(", ", labelAsInt16x2);

        public MainRecord parentRecord {
            get => _parentRecord;
            set {
                if (value == null) return;
                _parentRecord = value;
                _parentRecord.childGroup = this;
            }
        }

        public bool isChildGroup {
            get => header.groupType == 1 || 
                   header.groupType == 6 || 
                   header.groupType == 7;
        }
        public bool hasRecordParent {
            get => header.groupType == 1 || header.groupType >= 4;
        }

        public bool isChildGroupChild {
            get => header.groupType == 8 || header.groupType == 9;
        }

        public GroupRecord(Container container, PluginFileSource source)
            : base(container) {
            header = new TES4GroupHeader(source);
            if (signature != "GRUP")
                throw new Exception("Expected GRUP record");
            if (isChildGroup) SetParentRecord();
        }

        private void SetParentRecord() {
            var previousSibling = container.elements[index - 1];
            if (previousSibling is MainRecord rec && rec.formId == labelAsUInt32) {
                parentRecord = rec;
            } else {
                // this doesn't seem to happen at all in Skyrim.esm, but it's here just in case
                parentRecord = file.GetRecordByFormId(labelAsUInt32);
            }
        }

        public static GroupRecord Read(Container container, PluginFileSource source) {
            var group = new GroupRecord(container, source);
            var file = group.file;
            source.ReadMultiple(group.dataSize, () => {
                var sig = source.ReadSignature();
                source.stream.Seek(-4, SeekOrigin.Current);
                if (sig == GRUP) {
                    Read(group, source);
                } else {
                    var rec = MainRecord.Read(group, source, sig);
                    file.IndexRecord(rec);
                }
            });
            if (group._elements != null) group._elements.TrimExcess();
            return group;
        }

        private string GetName() {
            return groupType switch {
                GroupType.Top => labelAsSignature.ToString(),
                GroupType.WorldChildren => $"Children of {labelAsFormId:X8}",
                GroupType.InteriorCellBlock => $"Block {labelAsInt32}",
                GroupType.InteriorCellSubBlock => $"Sub-Block {labelAsInt32}",
                GroupType.ExteriorCellBlock => $"Block {coordinatesStr}",
                GroupType.ExteriorCellSubBlock => $"Subblock {coordinatesStr}",
                GroupType.CellChildren => $"Children of {labelAsFormId:X8}",
                GroupType.TopicChildren => $"Children of {labelAsFormId:X8}",
                GroupType.CellPersistentChildren => "Persistent",
                GroupType.CellTemporaryChildren => "Temporary",
                _ => throw new Exception("Unknown group type.")
            };
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
            if (!hasRecordParent) return null;
            if (isChildGroupChild && container is GroupRecord group)
                return group.GetParentRecord();
            return _parentRecord;
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

        public void IndexRecordsByEditorId() {
            recordsByEditorID = new EditorIdMap();
            foreach (var element in elements)
                if (element is MainRecord rec) 
                    recordsByEditorID.Add(rec);
        }
    }
}