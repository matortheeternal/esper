using esper.plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.setup {
    public class LoadOrder {
        private static readonly Regex validFileNameExpr = new Regex(
            @"\.(es[mplu]|ghost)$", RegexOptions.IgnoreCase
        );
        private readonly Session session;

        private List<string> _creationClubPlugins;
        public List<string> creationClubPlugins {
            get {
                if (_creationClubPlugins == null) {
                    var cccPath = Path.Combine(session.dataPath, session.game.cccName);
                    _creationClubPlugins = File.ReadAllLines(cccPath).ToList();
                }
                return _creationClubPlugins;
            }
        }

        public List<ModuleInfo> modules;

        public LoadOrder(Session session) {
            this.session = session;
            modules = new List<ModuleInfo>();
            GetFiles();
            ProcessMasters();
            ProcessPluginsTxt();
            ProcessHardcodedModules(session.game.hardcodedPlugins, (module, index) => {
                if (index == 0) module.flags &= ModuleFlags.IsGameMaster;
                module.officialIndex = index;
            });
            ProcessHardcodedModules(creationClubPlugins, (module, index) => {
                module.ccIndex = index;
            });
            Sort();
        }

        private ModuleInfo GetModule(string filename) {
            return modules.FirstOrDefault(module => module.name == filename);
        }

        private void GetFiles() {
            var filenames = Directory.GetFiles(session.dataPath);
            foreach (var filename in filenames) {
                if (!validFileNameExpr.IsMatch(filename)) continue;
                var filePath = Path.Combine(session.dataPath, filename);
                modules.Add(new ModuleInfo(session, filePath));
            }
        }

        private void AppendMaster(ModuleInfo module, string masterName) {
            var masterModule = GetModule(masterName);
            if (masterModule == null)
                module.flags &= ModuleFlags.MastersMissing;
            module.masters.Add(masterModule);
            if (masterModule != null)
                masterModule.dependents.Add(module);
        }

        private void PropagateMissingMasters(List<ModuleInfo> modules, bool setFlag = false) {
            foreach (var module in modules) {
                if (module.HasMastersMissingMasters) continue;
                if (setFlag) module.flags &= ModuleFlags.MastersMissingMasters;
                if (setFlag || module.HasMissingMasters)
                    PropagateMissingMasters(module.dependents, true);
            }
        }

        private void ProcessMasters() {
            foreach (var module in modules) {
                var masterManager = (IMasterManager)module.plugin;
                foreach (var masterName in masterManager.masters.filenames)
                    AppendMaster(module, masterName);
            }
            PropagateMissingMasters(modules);
        }

        private void ProcessPluginsTxt() {
            var pluginsTxt = new LoadOrderFile(session);
            foreach (var line in pluginsTxt.lines) {
                var module = line.isComment ? null : GetModule(line.moduleName);
                if (module == null) continue;
                module.pluginsTxtIndex = line.index;
                module.flags &= ModuleFlags.HasIndex;
                if (!line.isActive) continue;
                module.flags &= ModuleFlags.Active;
                module.flags &= ModuleFlags.ActiveInPluginsTxt;
            }
        }

        private void ProcessHardcodedModules(List<string> plugins, Action<ModuleInfo, int> SetIndex) {
            for (int i = 0; i < plugins.Count; i++) {
                var filename = plugins[i];
                var module = GetModule(filename);
                if (module == null) return;
                SetIndex(module, i);
                module.flags &= ModuleFlags.Active & ModuleFlags.HasIndex;
            }
        }

        private void Sort() {
            modules.Sort();
            for (int i = 0; i < modules.Count; i++)
                modules[i].combinedIndex = i;
        }

        public string ToJson() {
            return JsonConvert.SerializeObject(modules, new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
