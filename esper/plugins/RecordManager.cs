using esper.elements;
using esper.helpers;

namespace esper.plugins {
    [JSExport]
    public static class RecordManagerExtensions {
        public static void InitRecordMaps(this PluginFile m) {
            m.records = new List<MainRecord>((int)m.file.recordCount);
        }

        internal static MainRecord GetRecordExact(
            this PluginFile m, UInt32 formId
        ) {
            if (m.records == null || m.records.Count == 0) return null;
            return CollectionHelpers.BinarySearch(m.records, rec => {
                return formId.CompareTo(rec.fileFormId);
            });
        }

        public static MainRecord GetRecordByFormId(
            this PluginFile m, UInt32 formId
        ) {
            var hit = m.GetRecordExact(formId);
            if (hit != null) return hit;
            if (!(m.session?.options?.allowInjections ?? true)) return null;
            var slot = (byte)(formId >> 24);
            if (slot < (m.originalMasters?.Count ?? 0)) return null;
            return m.GetInjectedRecord(formId);
        }

        public static MainRecord GetRecordByLocalFormId(
            this PluginFile m, UInt32 localFormId
        ) {
            if (m.file.isDummy) return null;
            if (m.records == null || m.records.Count == 0) return null;
            var targetOrdinal = m.file.FileToOrdinal(m.file, false);
            return CollectionHelpers.BinarySearch(m.records, rec => {
                var recOrdinal = (byte)(rec.fileFormId >> 24);
                var ordinalComparison = targetOrdinal.CompareTo(recOrdinal);
                return ordinalComparison < 0
                    ? ordinalComparison
                    : localFormId.CompareTo(rec.localFormId);
            });
        }

        public static void IndexRecord(this PluginFile m, MainRecord rec) {
            m.records.Add(rec);
        }

        public static UInt32 GetHighObjectID(this PluginFile m) {
            var highRecord = CollectionHelpers.BinarySearch(m.records, rec => {
                return rec.fileFormId > 0xFFFFFF ? -1 : 1;
            }, true);
            return highRecord.fileFormId > 0xFFFFFF ? 0x800 : highRecord.fileFormId;
        }

        public static void SortRecords(this PluginFile m) {
            m.records.Sort((rec1, rec2) => {
                return rec1.fileFormId.CompareTo(rec2.fileFormId);
            });
        }
    }
}
