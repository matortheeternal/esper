using esper.defs;
using esper.data;
using esper.plugins;
using esper.resolution;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using esper.data.headers;

namespace esper.elements {
    public class MainRecord : Container, IMainRecord {
        internal readonly IRecordHeader header;
        internal List<Subrecord> _unexpectedSubrecords;

        internal readonly PluginFile _file;
        internal readonly long bodyOffset;
        internal byte[] decompressedData;

        public MainRecordDef mrDef => def as MainRecordDef;
        public override MainRecord record => this;
        public override PluginFile file => _file;
        public GroupRecord childGroup { get; set; }

        public UInt32 formId => header.formId;
        public UInt32 localFormId => formId & 0xFFFFFF;

        public UInt32 dataSize => compressed
                    ? (uint) decompressedData.Length
                    : header.dataSize;
        public bool compressed => this.GetRecordFlag("Compressed");
        public string editorId => this.GetValue("EDID");

        public ReadOnlyCollection<Subrecord> unexpectedSubrecords {
            get => _unexpectedSubrecords.AsReadOnly();
        }

        public override ReadOnlyCollection<Element> elements {
            get {
                if (_internalElements == null)
                    mrDef.ReadElements(this, file.source);
                return internalElements.AsReadOnly();
            }
        }

        public MainRecord(Container container, ElementDef def) 
            : base(container, def) {}

        public override void Initialize() {
            mrDef.InitElement(this);
        }

        public MainRecord(Container container, ElementDef def, PluginFileSource source)
            : base(container, def) {
            _file = container.file;
            header = manager.headerManager.ReadRecordHeader(source);
            bodyOffset = source.stream.Position;
            if (sessionOptions.readAllSubrecords)
                mrDef.ReadElements(this, source);
            source.stream.Position = bodyOffset + header.dataSize;
        }

        public static MainRecord Read(
            Container container,
            PluginFileSource source,
            Signature signature
        ) {
            var def = (ElementDef) container.manager.GetRecordDef(signature);
            var record = new MainRecord(container, def, source);
            return record;
        }

        internal bool Decompress(PluginFileSource source) {
            if (decompressedData == null)
                decompressedData = source.Decompress(header.dataSize);
            source.SetDecompressedStream(decompressedData);
            if (decompressedData != null) return true;
            return false;
        }

        public bool IsLocal() {
            var masterCount = (_file as IMasterManager).originalMasters.Count;
            return (formId >> 24) >= masterCount;
        }

        public override bool SupportsSignature(string sig) {
            return mrDef.memberDefs.Any(d => d.HasSignature(sig));
        }

        internal override void ElementsReady() {
            base.ElementsReady();
            Initialize();
        }
    }
}
