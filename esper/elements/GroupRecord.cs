using esper.data;
using esper.data.headers;
using esper.defs;
using esper.io;
using esper.plugins;

namespace esper.elements {
    [JSExport]
    public class GroupRecord : Container {
        public TES4GroupHeader header;
        public EditorIdMap recordsByEditorID;
        private MainRecord _parentRecord;

        public GroupDef groupDef => (GroupDef)def;
        public override string name => groupDef.GetName(this);
        public override string displayName => groupDef.GetDisplayName(this);
        public override GroupRecord group => this;

        public override UInt32 size {
            get => (UInt32)(groupHeaderSize + internalElements.Sum(e => e.size));
        }

        public UInt32 groupSize => header.groupSize;
        public int groupType => header.groupType;
        internal object label => groupDef.ConvertLabel(this, header.label);
        public UInt32 dataSize => groupSize - groupHeaderSize;

        public bool isTopGroup => groupDef.isTopGroup;
        public bool hasRecordParent => groupDef.hasRecordParent;
        public bool isChildGroup => groupDef.isChildGroup;
        public bool isChildGroupChild => groupDef.isChildGroupChild;

        private UInt32 groupHeaderSize => manager.game.groupHeaderSize;

        public MainRecord parentRecord {
            get => _parentRecord;
            set {
                if (value == null) return;
                _parentRecord = value;
                _parentRecord.childGroup = this;
            }
        }

        public GroupRecord(
            Container container, GroupDef def, TES4GroupHeader header
        ) : base(container, def) {
            this.header = header;
        }

        public GroupRecord(
            Container container, GroupDef def, byte[] label
        ) : base(container, def) {
            // Size from the def (not the container chain): new groups are
            // constructed with a null container to skip Element's auto-add,
            // then positioned manually by the caller.
            var headerSize = def.manager.game.groupHeaderSize;
            header = new TES4GroupHeader(label, def.groupType, headerSize);
        }

        public override bool SupportsSignature(Signature sig) {
            return groupDef.signature == sig;
        }

        internal override bool RemoveElement(Element element) {
            if (!base.RemoveElement(element)) return false;
            if (element is MainRecord rec) {
                if (file != null) file.UnindexRecord(rec);
                recordsByEditorID?.Remove(rec);
            }
            return true;
        }

        private void SetParentRecord() {
            if (!(label is FormId fid)) return;
            int index = container.internalElements.Count - 2;
            if (index > -1 && container.internalElements[index] is MainRecord rec) {
                if (rec.fileFormId == fid.fileFormId) {
                    parentRecord = rec;
                    return;
                }
            }
            // this doesn't seem to happen at all in Skyrim.esm, but it's here just in case
            parentRecord = file.GetRecordByFormId(fid.fileFormId);
        }

        public static GroupRecord Read(Container container, PluginFileSource source) {
            TES4GroupHeader header = new TES4GroupHeader(source);
            var groupDef = container.def.GetGroupDef(header);
            var group = new GroupRecord(container, groupDef, header);
            if (group.groupDef.isChildGroup) group.SetParentRecord();
            group.groupDef.ReadChildren(group, source);
            return group;
        }

        public MainRecord GetParentRecord() {
            if (!groupDef.hasRecordParent) return null;
            if (groupDef.isChildGroupChild && container is GroupRecord group)
                return group.GetParentRecord();
            return _parentRecord;
        }

        public void IndexRecordsByEditorId() {
            recordsByEditorID = new EditorIdMap();
            foreach (var element in elements)
                if (element is MainRecord rec)
                    recordsByEditorID.Add(rec);
        }

        internal override void WriteTo(PluginFileOutput output) {
            header.groupSize = size;
            header.WriteTo(output);
            output.WriteContainer(this);
        }

        internal override Element CreateDefault() {
            var targetDef = groupDef.childDefs.SingleOrDefault(def => {
                return def is MainRecordDef;
            });
            return targetDef.NewElement(this);
        }

        public override Element CopyTo(Element target, CopyOptions options) {
            // TODO: copy child group into MainRecord?
            if (target is PluginFile && target.def.ChildDefSupported(def))
                return target.AssignCopy(this, options);
            throw new Exception($"Cannot copy group records into ${target.def.displayName}");
        }

        internal override JToken ToJson() {
            if (!isChildGroup) return ToJsonArray();
            return new JObject {
                ["groupType"] = groupType,
                ["label"] = label.ToString(),
                ["elements"] = ToJsonArray()
            };
        }
    }
}
