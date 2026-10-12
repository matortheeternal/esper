namespace esper.setup {
    [JSExport]
    public class LoadOrder {
        private static readonly Regex validFileNameExpr = new Regex(
            @"\.(es[mplu]|ghost)$", RegexOptions.IgnoreCase
        );
        public Session session { get; internal set; }

        private List<string> _creationClubPlugins;
        public List<string> creationClubPlugins {
            get {
                if (_creationClubPlugins == null) {
                    _creationClubPlugins = new List<string>();
                    if (string.IsNullOrWhiteSpace(session.game.cccName))
                        return _creationClubPlugins;
                    if (string.IsNullOrWhiteSpace(session.dataPath)) {
                        session.logger.Warn(
                            "Cannot load Creation Club plugins without a data path."
                        );
                        return _creationClubPlugins;
                    }
                    var cccPath = Path.Combine(session.dataPath, session.game.cccName);
                    if (!File.Exists(cccPath)) {
                        session.logger.Info(
                            $"Creation Club plugin file not found: {cccPath}"
                        );
                        return _creationClubPlugins;
                    }
                    _creationClubPlugins.AddRange(File.ReadAllLines(cccPath));
                }
                return _creationClubPlugins;
            }
        }

        public List<ModuleInfo> modules { get; }

        public LoadOrder(Session session) {
            this.session = session;
            modules = new List<ModuleInfo>();
            GetFiles();
            ProcessMasters();
            ProcessPluginsTxt();
            ProcessHardcodedModules(session.game.hardcodedPlugins, (module, index) => {
                if (index == 0) module.flags |= ModuleFlags.IsGameMaster;
                module.officialIndex = index;
            });
            ProcessHardcodedModules(creationClubPlugins, (module, index) => {
                module.ccIndex = index;
            });
            Sort();
        }

        private ModuleInfo GetModule(string filename) {
            return modules.FirstOrDefault(module => string.Equals(
                module.name, filename, StringComparison.OrdinalIgnoreCase
            ));
        }

        private void GetFiles() {
            if (string.IsNullOrWhiteSpace(session.dataPath)) {
                session.logger.Warn("Cannot build load order without a data path.");
                return;
            }
            if (!Directory.Exists(session.dataPath)) {
                session.logger.Warn(
                    $"Cannot build load order; data path not found: {session.dataPath}"
                );
                return;
            }

            try {
                foreach (var filePath in Directory.EnumerateFiles(session.dataPath)) {
                    if (!validFileNameExpr.IsMatch(filePath)) continue;
                    modules.Add(new ModuleInfo(session, filePath));
                }
            } catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException
            ) {
                session.logger.Error(
                    $"Could not enumerate data path {session.dataPath}: " +
                    exception.Message
                );
            }
        }

        private void AppendMaster(ModuleInfo module, string masterName) {
            var masterModule = GetModule(masterName);
            if (masterModule == null)
                module.flags |= ModuleFlags.MastersMissing;
            module.masters.Add(masterModule);
            if (masterModule != null)
                masterModule.dependents.Add(module);
        }

        private void PropagateMissingMasters(List<ModuleInfo> modules, bool setFlag = false) {
            foreach (var module in modules) {
                if (module.HasMastersMissingMasters) continue;
                if (setFlag) module.flags |= ModuleFlags.MastersMissingMasters;
                if (setFlag || module.HasMissingMasters)
                    PropagateMissingMasters(module.dependents, true);
            }
        }

        private void ProcessMasters() {
            foreach (var module in modules) {
                foreach (var masterName in module.plugin.masters.filenames)
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
                module.flags |= ModuleFlags.HasIndex;
                if (!line.isActive) continue;
                module.flags |= ModuleFlags.Active;
                module.flags |= ModuleFlags.ActiveInPluginsTxt;
            }
        }

        private void ProcessHardcodedModules(List<string> plugins, Action<ModuleInfo, int> SetIndex) {
            for (int i = 0; i < plugins.Count; i++) {
                var filename = plugins[i];
                var module = GetModule(filename);
                if (module == null) continue;
                SetIndex(module, i);
                module.flags |= ModuleFlags.Active | ModuleFlags.HasIndex;
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
