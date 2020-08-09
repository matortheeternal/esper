using esper.elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.plugins {
    public class FormIdMap {
        private readonly SortedDictionary<UInt32, MainRecord> _map;

        public UInt32 highObjectId => _map.Keys.Last();

        public FormIdMap() {
            _map = new SortedDictionary<uint, MainRecord>();
        }

        public MainRecord Get(UInt32 formId) {
            return _map[formId];
        }

        public void Add(UInt32 formId, MainRecord rec) {
            _map[formId] = rec;
        }
    }

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
            _plugins[file].Add(record.localFormId, record);
        }
    }
}
