using esper.elements;
using System;
using System.Collections.Generic;

namespace esper.plugins {
    public class RecordMap<T> : SortedDictionary<T, MainRecord> {
        readonly Func<MainRecord, T> GetKey;

        public RecordMap(Func<MainRecord, T> GetKey) {
            this.GetKey = GetKey;
        }

        public void Add(MainRecord record) {
            this[GetKey(record)] = record;
        }
    }

    public class PluginRecordMap<T> : Dictionary<PluginFile, RecordMap<T>> {
        readonly Func<MainRecord, T> GetKey;

        public PluginRecordMap(Func<MainRecord, T> GetKey) {
            this.GetKey = GetKey;
        }

        public void AddMap(PluginFile file) {
            this[file] = new RecordMap<T>(GetKey);
        }

        public void Add(MainRecord record) {
            if (!ContainsKey(record.file)) AddMap(record.file);
            this[record.file].Add(record);
        }
    }
}
