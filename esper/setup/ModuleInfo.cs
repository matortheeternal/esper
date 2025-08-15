using esper.plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace esper.setup {
    public class ModuleInfo {
        private static readonly int GHOST_EXT_LENGTH = 6;

        [JsonIgnore]
        public Session session;
        public string filePath;
        public string originalName;
        public string name;
        public DateTime lastModified;
        public ModuleExtension extension;
        public List<string> masterNames;
        [JsonIgnore]
        public List<ModuleInfo> masters;
        [JsonIgnore]
        public List<ModuleInfo> dependents;
        public ModuleFlags flags = 0;
        public int? officialIndex = null;
        public int? ccIndex = null;
        public int? pluginsTxtIndex = null;
        public int? combinedIndex = null;
        [JsonIgnore]
        public PluginFile plugin;

        private List<ModuleExtension> SupportedExtensions {
            get => session.game.pluginExtensions;
        }

        private bool HasUnsupportedExtension {
            get => !SupportedExtensions.Contains(extension);
        }

        private bool IsAutomaticESM {
            get => SupportedExtensions.Contains(ModuleExtension.ESL) &&
                extension == ModuleExtension.ESM ||
                extension == ModuleExtension.ESL;
        }

        public bool HasMissingMasters {
            get => (flags & ModuleFlags.MastersMissing) > 0;
        }

        public bool HasMastersMissingMasters {
            get => (flags & ModuleFlags.MastersMissingMasters) > 0;
        }

        public ModuleInfo(Session session, string filePath) {
            this.filePath = filePath;
            this.session = session;
            originalName = Path.GetFileName(filePath);
            name = originalName;
            lastModified = File.GetLastWriteTime(filePath);
            plugin = session.pluginManager.LoadPluginHeader(filePath);
            ProcessExtension();
            ProcessFileFlags();
        }

        private int? CompareIndexes(int? index1, int? index2) {
            if (index1 == null && index2 == null) return null;
            if (index1 != null && index2 == null) return -1;
            if (index1 == null && index2 != null) return 1;
            return (int)index1 - (int)index2;
        }

        public int CompareTo(ModuleInfo other) {
            return CompareIndexes(officialIndex, other.officialIndex)
                ?? CompareIndexes(ccIndex, other.ccIndex)
                ?? CompareIndexes(pluginsTxtIndex, other.pluginsTxtIndex)
                ?? 0;
        }

        private void ProcessExtension() {
            var extString = Path.GetExtension(name).ToLower();
            if (extString == "ghost") {
                name = name.Substring(0, name.Length - GHOST_EXT_LENGTH);
                flags &= ModuleFlags.Ghost;
                if (session.pluginManager.HasPlugin(name))
                    throw new Exception("Ignoring ghost plugin: original exists.");
            }
            extension = Path.GetExtension(name).ToLower() switch {
                "esp" => ModuleExtension.ESP,
                "esm" => ModuleExtension.ESM,
                "esl" => ModuleExtension.ESL,
                "esu" => ModuleExtension.ESU,
                _ => ModuleExtension.Unknown
            };
            if (HasUnsupportedExtension) 
                throw new Exception("Unsupported module extension");
            if (IsAutomaticESM)
                flags &= ModuleFlags.HasESMExtension & ModuleFlags.IsESM;
            if (extension == ModuleExtension.ESL) flags &= ModuleFlags.HasESLFlag;
        }

        private void ProcessFileFlags() {
            if (plugin.esm) flags &= ModuleFlags.HasESMFlag;
            if (plugin.esl) flags &= ModuleFlags.HasESLFlag;
            if (plugin.localized) flags &= ModuleFlags.HasLocalizedFlag;
            flags &= ModuleFlags.Valid;
        }
    }
}
