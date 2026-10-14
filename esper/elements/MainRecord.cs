using esper.data;
using esper.data.headers;
using esper.defs;
using esper.io;
using esper.plugins;
using esper.resolution;
using esper.setup;
using System.Collections.ObjectModel;

namespace esper.elements {
    [JSExport]
    public class MainRecord : Container, IMainRecord {
        private TES4RecordHeader _header;

        internal readonly PluginFile _file;
        internal readonly long bodyOffset;
        internal RecordSource recordSource;
        internal List<MainRecord> _overrides;
        internal List<Subrecord> _unexpectedSubrecords;
        internal List<MainRecord> _referencedBy;

        internal TES4RecordHeader header {
            get => _header;
            set {
                _header = value;
                _overrides = new List<MainRecord>();
            }
        }

        public override string pathKey => fileFormId.ToString("X8");

        public MainRecordDef mrDef => def as MainRecordDef;
        public override MainRecord record => this;
        public override PluginFile file => _file;
        internal PluginManager pluginManager => _file.session.pluginManager;
        public GroupRecord childGroup { get; set; }
        public Element headerElement => this.GetElement("Record Header");

        public FormId formId => FormId.FromSource(_file, fileFormId);
        public UInt32 fileFormId => _header.formId;
        public UInt32 localFormId => fileFormId & 0xFFFFFF;
        public UInt32? globalFormId => formId.globalFormId;

        public MainRecord master => local ? this : formId.ResolveRecord();
        public ReadOnlyCollection<MainRecord> overrides {
            get {
                var m = local ? this : master;
                if (m == null || ReferenceEquals(m, this)) return _overrides.AsReadOnly();
                return m.overrides;
            }
        }
        public ReadOnlyCollection<MainRecord> referencedBy {
            get {
                var m = local ? this : master;
                if (m == null || ReferenceEquals(m, this)) {
                    if (_referencedBy == null) _referencedBy = new List<MainRecord>();
                    return _referencedBy.AsReadOnly();
                }
                return m.referencedBy;
            }
        }

        public UInt32 dataSize => compressed
                    ? recordSource.dataSize
                    : header.dataSize;
        public bool compressed => this.GetRecordFlag(18);

        public string editorId {
            get => this.GetValue("EDID");
            set => this.SetValue("EDID", value);
        }

        public string fullName {
            get => this.GetValue("FULL");
            set => this.SetValue("FULL", value);
        }
        public override string name {
            get => this.GetValue("[FULL|EDID]") ?? formId.ToString();
        }

        public bool local {
            get {
                if (fileFormId == 0) return true;
                var masterCount = _file.originalMasters?.Count ?? 0;
                return (fileFormId >> 24) >= masterCount;
            }
        }

        public bool isInjected {
            get {
                if (local || fileFormId == 0) return false;
                var slot = (byte)(fileFormId >> 24);
                if (slot >= (_file.originalMasters?.Count ?? 0)) return false;
                var masterFile = _file.OrdinalToFile(slot, false);
                return masterFile != null && masterFile != _file
                    && masterFile.GetRecordExact(fileFormId) == null;
            }
        }

        public ReadOnlyCollection<Subrecord> unexpectedSubrecords {
            get {
                if (_unexpectedSubrecords == null)
                    mrDef.ReadElements(this, file.source);
                return _unexpectedSubrecords.AsReadOnly();
            }
        }

        public override List<Element> elements {
            get {
                if (_internalElements == null)
                    mrDef.ReadElements(this, file.source);
                return internalElements.ToList();
            }
        }

        public MainRecord(Container container, ElementDef def)
            : base(container, def) { }

        public override void Initialize() {
            mrDef.InitElement(this);
        }

        public MainRecord(Container container, ElementDef def, PluginFileSource source, TES4RecordHeader header)
            : base(container, def) {
            _file = container.file;
            this.header = header;
            bodyOffset = source.stream.Position;
            if (sessionOptions.readAllSubrecords)
                mrDef.ReadElements(this, source);
            if (!local) {
                var m = master;
                if (m == null || ReferenceEquals(m, this)) {
                    if (sessionOptions.allowInjections) _file.RegisterInjection(this);
                } else {
                    m.AddOverride(this);
                }
            }
            source.stream.Position = bodyOffset + header.dataSize;
        }

        public static MainRecord Read(
            Container container,
            PluginFileSource source,
            Signature signature
        ) {
            var header = new TES4RecordHeader(source);
            var def = container.def.GetMainRecordDef(header);
            return new MainRecord(container, def, source, header);
        }

        internal void AddOverride(MainRecord ovr) {
            if (_overrides == null)
                throw new Exception("Internal overrides array hasn't been initialized.  " +
                    "Is the record not local or did you not initialize it for a new record?");
            _overrides.Add(ovr);
        }

        public override bool SupportsSignature(Signature sig) {
            return mrDef.memberDefs.Any(d => d.HasSignature(sig));
        }

        internal override void ElementsReady() {
            base.ElementsReady();
            Initialize();
        }

        public override void BuildRefBy() {
            var startIndex = mrDef.GetFirstRealElementIndex(this);
            for (int i = startIndex; i < elements.Count; i++) {
                var element = elements[i];
                if (element.def.canContainFormIds) element.BuildRefBy();
            }
            _internalElements = null;
        }

        internal void DestroyRefBy(PluginFile plugin) {
            if (_referencedBy == null) return;
            _referencedBy.RemoveAll(r => r._file.Equals(plugin));
        }

        internal void AddRef(MainRecord rec) {
            if (_referencedBy == null)
                _referencedBy = new List<MainRecord>();
            _referencedBy.Add(rec);
        }

        internal override Element ResolveIn(Element container) {
            foreach (var element in container.internalElements) {
                if (element is MainRecord rec && rec.formId == formId) {
                    if (rec.signature != signature)
                        throw new Exception("Signature mismatch.");
                    return rec;
                }
            }
            return null;
        }

        internal GroupDef GetChildGroupDef() {
            if (container.def is GroupDef parentGroupDef)
                return parentGroupDef.GetChildGroupDef();
            return null;
        }

        internal GroupRecord CreateChildGroup() {
            if (childGroup != null) return childGroup;
            var childGroupDef = GetChildGroupDef();
            if (childGroupDef == null)
                throw new Exception($"Cannot create child group for {signature}");
            var label = BitConverter.GetBytes(fileFormId);
            childGroup = new GroupRecord(null, childGroupDef, label) {
                container = container
            };
            var index = container.internalElements.IndexOf(this) + 1;
            container.internalElements.Insert(index, childGroup);
            return childGroup;
        }

        internal override Element CopyInto(Element container, CopyOptions options) {
            var rec = new MainRecord(container as Container, def);
            CopyChildrenInto(rec, options);
            mrDef.UpdateContainedIn(rec);
            if ((options & CopyOptions.CopyChildGroups) > 0)
                childGroup.CopyInto(rec, options);
            return rec;
        }

        public override Element CopyTo(Element target, CopyOptions options) {
            if (target is PluginFile) return target.AssignCopy(this, options);
            if (target is GroupRecord && target.def.ChildDefSupported(def))
                return target.AssignCopy(this, options);
            throw new Exception($"Cannot copy records into ${target.def.displayName}");
        }
    }
}
