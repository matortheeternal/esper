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

        public void Add(MainRecord rec) {
            _map[rec.localFormId] = rec;
        }
    }
}
