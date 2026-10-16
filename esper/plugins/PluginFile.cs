using balsa.stringtables;
using esper.data;
using esper.defs;
using esper.elements;
using esper.helpers;
using esper.io;
using esper.resolution;
using esper.setup;

namespace esper.plugins {
    [JSExport]
    public class PluginFile : Container {
        public MainRecord header { get; internal set; }
        internal Session session;
        internal string filename;
        internal PluginFileOptions options;
        internal PluginFileSource source;
        internal PluginSlot pluginSlot;
        internal Dictionary<StringFileType, StringFile> stringFiles;

        public PluginFileDef pluginDef => (PluginFileDef)def;
        public bool isDummy => source == null;
        public uint recordCount => header.GetData<uint>(@"HEDR\Number of Records");

        public ReadOnlyMasterList originalMasters { get; internal set; }
        public MasterList masters { get; internal set; }
        public bool mastersChanged { get; internal set; }

        public List<MainRecord> records { get; internal set; }

        internal List<MainRecord> injectedRecords;

        internal void RegisterInjection(MainRecord rec) {
            var slot = (byte)(rec.fileFormId >> 24);
            if (slot >= (originalMasters?.Count ?? 0)) return;
            var masterFile = this.OrdinalToFile(slot, false);
            if (masterFile == null || masterFile == this) return;
            if (masterFile.GetRecordExact(rec.fileFormId) != null) return;
            masterFile.AddInjected(rec);
        }

        internal void AddInjected(MainRecord rec) {
            injectedRecords ??= new List<MainRecord>();
            rec._overrides ??= new List<MainRecord>();
            if (injectedRecords.Count == 0) {
                injectedRecords.Add(rec);
                return;
            }
            var existing = CollectionHelpers.BinarySearch(injectedRecords, r => {
                return rec.fileFormId.CompareTo(r.fileFormId);
            });
            if (existing != null) {
                existing.AddOverride(rec);
                return;
            }
            injectedRecords.Add(rec);
            injectedRecords.Sort((a, b) => {
                return a.fileFormId.CompareTo(b.fileFormId);
            });
        }

        internal MainRecord GetInjectedRecord(UInt32 formId) {
            if (injectedRecords == null || injectedRecords.Count == 0) return null;
            return CollectionHelpers.BinarySearch(injectedRecords, rec => {
                return formId.CompareTo(rec.fileFormId);
            });
        }

        public override PluginFile file => this;
        public override string name => filename;
        public string filePath => source?.filePath;
        public new DefinitionManager manager => session.definitionManager;
        public bool localized => header.GetRecordFlag("Localized");
        public bool esl => header.GetRecordFlag("ESL");
        public bool esm => header.GetRecordFlag("ESM");

        public PluginFile(Session session, string filename, PluginFileOptions options)
            : base(session.root, session.pluginFileDef) {
            this.session = session;
            this.filename = filename;
            this.options = options;
        }

        public void Save(string filePath) {
            this.CheckMasters();
            this.UpdateMastersElement();
            this.UpdateHeaderRecordCount();
            var output = new PluginFileOutput(filePath, this);
            WriteTo(output);
            output.stream.Flush();
            output.stream.Close();
        }

        internal void UpdateHeaderRecordCount() {
            header.SetData<UInt32>(
                @"HEDR\Number of Records", CountedRecordCount() - 1);
        }

        private UInt32 CountedRecordCount() {
            UInt32 total = 0;
            foreach (var element in internalElements)
                total += CountedRecords(element);
            return total;
        }

        private static UInt32 CountedRecords(Element element) {
            if (element is MainRecord) return 1;
            if (element is Container container) {
                UInt32 total = element is GroupRecord ? 1u : 0u;
                foreach (var child in container.internalElements)
                    total += CountedRecords(child);
                return total;
            }
            return 0;
        }

        internal bool UseDLStrings(Signature rSig, Signature eSig) {
            return (rSig != Signatures.LSCR && eSig == Signatures.DESC) ||
                (rSig == Signatures.QUST && eSig == Signatures.CNAM) ||
                (rSig == Signatures.BOOK && eSig == Signatures.CNAM);
        }

        internal bool UseILStrings(Signature rSig, Signature eSig) {
            return rSig == Signatures.INFO && eSig == Signatures.RNAM;
        }

        internal StringFileType GetStringFileType(Element element) {
            var elementSig = element.signature;
            var recordSig = element.record.signature;
            if (UseDLStrings(recordSig, elementSig))
                return StringFileType.DLSTRINGS;
            if (UseILStrings(recordSig, elementSig))
                return StringFileType.ILSTRINGS;
            return StringFileType.STRINGS;
        }

        internal string GetString(UInt32 id, Element element) {
            if (!localized) throw new Exception("Plugin is not localized.");
            var sf = stringFiles[GetStringFileType(element)];
            if (sf != null && sf.strings.TryGetValue(id, out string str))
                return str;
            return $"<Could not find string #{id}>";
        }

        public override bool SupportsSignature(Signature sig) {
            return pluginDef.IsTopGroup(sig);
        }

        internal override Element CreateElementBySignature(string sig) {
            var signature = Signature.FromString(sig);
            if (!pluginDef.topGroups.TryGetValue(signature, out var groupDef))
                throw new Exception($"{sig} is not a supported top group in {name}");
            var group = elements.OfType<GroupRecord>().FirstOrDefault(g =>
                g.groupType == 0 && g.label.ToString() == sig);
            if (group == null)
                group = groupDef.CreateFromName(this, sig);
            return group;
        }

        public override bool Remove() {
            throw new Exception("Cannot remove plugin files.");
        }

        public override void BuildRefBy() {
            foreach (var rec in records)
                rec.BuildRefBy();
        }

        internal override JToken ToJson() {
            var o = new JObject {
                { "Filename", filename },
                { "File Header", header.ToJson() }
            };
            foreach (var e in elements) {
                if (e is MainRecord) continue;
                o[e.name] = e.ToJson();
            }
            return o;
        }
    }
}
