using esper.setup;
using esper.elements;
using System.Text;
using System;
using esper.resolution;
using System.Collections.Generic;
using esper.data;

namespace esper.plugins {
    public class PluginFile : Container, IMasterManager, IRecordManager {
        private readonly Signature TES4 = Signature.FromString("TES4");

        public MainRecord header;
        public Session session;
        public string filename;
        public PluginFileOptions options;
        internal PluginFileSource source;

        public bool isDummy => source == null;
        public uint recordCount => header.GetData(@"HEDR\Number of Records");
        public Encoding stringEncoding => session.options.encoding;

        PluginFile IMasterManager.file => this;
        ReadOnlyMasterList IMasterManager.originalMasters { get; set; }
        MasterList IMasterManager.masters { get; set; }
        bool IMasterManager.mastersChanged { get; set; }

        PluginFile IRecordManager.file => this;
        List<MainRecord> IRecordManager.records { get; set; }

        public override PluginFile file => this;
        public string filePath => source?.filePath;
        public new DefinitionManager manager => session.definitionManager;
        public bool localized => header.GetRecordFlag("Localized");

        public PluginFile(Session session, string filename, PluginFileOptions options)
            : base() {
            this.session = session;
            this.filename = filename;
            this.options = options;
            session.pluginManager.AddFile(this);
        }

        public bool IsEsl() {
            return false; // TODO
        }

        public void Save(string filePath) {
            this.CheckMasters();
            this.UpdateMastersElement();
            var output = new PluginFileOutput(filePath, this);
            internalElements.ForEach(element => {
                element.WriteTo(output);
            });
            output.stream.Flush();
            output.stream.Close();
        }

        internal void ReadFileHeader() {
            if (header != null) return;
            header = MainRecord.Read(this, source, TES4);
            this.InitMasters();
            this.InitRecordMaps();
        }

        internal void ReadGroups() {
            var endOffset = source.fileSize - 1;
            while (source.stream.Position < endOffset)
                GroupRecord.Read(this, source);
            internalElements.TrimExcess();
            this.SortRecords();
        }

        internal string GetString(uint id) {
            throw new NotImplementedException();
        }

        public override bool SupportsSignature(string sig) {
            return manager.IsTopGroup(sig);
        }

        internal override void WriteTo(PluginFileOutput output) {
            throw new NotImplementedException();
        }
    }
}
