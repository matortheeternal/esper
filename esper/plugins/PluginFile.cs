using esper.setup;
using esper.elements;
using System;
using esper.resolution;
using System.Collections.Generic;
using esper.defs;
using balsa.stringtables;

namespace esper.plugins {
    public class PluginFile : Container, IMasterManager, IRecordManager {
        public MainRecord header;
        internal Session session;
        internal string filename;
        internal PluginFileOptions options;
        internal PluginFileSource source;
        internal PluginSlot pluginSlot;
        internal List<StringFile> stringFiles;

        public PluginFileDef pluginDef => (PluginFileDef)def;
        public bool isDummy => source == null;
        public uint recordCount => header.GetData(@"HEDR\Number of Records");

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
        public bool esl => header.GetRecordFlag("ESL");
        public bool esm => header.GetRecordFlag("ESM");

        public PluginFile(Session session, string filename, PluginFileOptions options)
            : base(session.root, session.pluginFileDef) {
            this.session = session;
            this.filename = filename;
            this.options = options;
            session.pluginManager.AddFile(this);
        }

        public void Save(string filePath) {
            this.CheckMasters();
            this.UpdateMastersElement();
            var output = new PluginFileOutput(filePath, this);
            WriteTo(output);
            output.stream.Flush();
            output.stream.Close();
        }

        internal string GetString(UInt32 id) {
            if (!localized) throw new Exception("Plugin is not localized.");
            if (stringFiles != null)
                foreach (var stringFile in stringFiles)
                    if (stringFile.strings.TryGetValue(id, out string str))
                        return str;
            return $"<Could not find string #{id}>";
        }

        public override bool SupportsSignature(string sig) {
            return pluginDef.IsTopGroup(sig);
        }

        public override bool Remove() {
            throw new Exception("Cannot remove plugin files.");
        }
    }
}
