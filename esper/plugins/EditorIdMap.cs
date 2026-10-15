using esper.elements;

namespace esper.plugins {
    [JSExport]
    public class EditorIdMap {
        private readonly SortedDictionary<string, MainRecord> _map;

        public EditorIdMap() {
            _map = new SortedDictionary<string, MainRecord>();
        }

        public MainRecord Get(string editorId) {
            return _map[editorId];
        }

        public void Add(MainRecord rec) {
            var editorId = rec.editorId;
            if (editorId == null) return;
            _map[editorId] = rec;
        }

        public void Remove(MainRecord rec) {
            var editorId = rec.editorId;
            if (editorId == null) return;
            if (_map.TryGetValue(editorId, out var current) && current == rec)
                _map.Remove(editorId);
        }
    }
}
