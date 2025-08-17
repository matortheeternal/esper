using esper.elements;

namespace esper.plugins {
    public class PluginRecordMap {
        private readonly Dictionary<PluginFile, FormIdMap> _plugins;

        public PluginRecordMap() {
            _plugins = new Dictionary<PluginFile, FormIdMap>();
        }

        public void AddMap(PluginFile file) {
            _plugins[file] = new FormIdMap();
        }

        public FormIdMap GetMap(PluginFile file) {
            return _plugins[file];
        }

        public void Add(PluginFile file, MainRecord record) {
            if (!_plugins.ContainsKey(file)) AddMap(file);
            _plugins[file].Add(record);
        }
    }
}
