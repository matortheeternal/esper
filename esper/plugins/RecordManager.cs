using esper.elements;
using esper.helpers;

namespace esper.plugins {
    [JSExport]
    public interface IRecordManager {
        public List<MainRecord> records { get; set; }
        internal PluginFile file { get; }
    }

    [JSExport]
    public static class RecordManagerExtensions {
        public static void InitRecordMaps(this IRecordManager m) {
            m.records = new List<MainRecord>((int)m.file.recordCount);
        }

        public static MainRecord GetRecordByFormId(
            this IRecordManager m, UInt32 formId
        ) {
            if (m.file.isDummy) return null;
            return CollectionHelpers.BinarySearch(m.records, rec => {
                return formId.CompareTo(rec.fileFormId);
            });
        }

        public static MainRecord GetRecordByLocalFormId(
            this IRecordManager m, UInt32 localFormId
        ) {
            if (m.file.isDummy) return null;
            var targetOrdinal = m.file.FileToOrdinal(m.file, false);
            return CollectionHelpers.BinarySearch(m.records, rec => {
                var recOrdinal = (byte) (rec.fileFormId >> 24);
                var ordinalComparison = targetOrdinal.CompareTo(recOrdinal);
                return ordinalComparison < 0 
                    ? ordinalComparison
                    : localFormId.CompareTo(rec.localFormId);
            });
        }

        public static void IndexRecord(this IRecordManager m, MainRecord rec) {
            m.records.Add(rec);
        }

        public static UInt32 GetHighObjectID(this IRecordManager m) {
            var highRecord = CollectionHelpers.BinarySearch(m.records, rec => {
                return rec.fileFormId > 0xFFFFFF ? -1 : 1;
            }, true);
            return highRecord.fileFormId > 0xFFFFFF ? 0x800 : highRecord.fileFormId;
        }

        public static void SortRecords(this IRecordManager m) {
            m.records.Sort((rec1, rec2) => {
                return rec1.fileFormId.CompareTo(rec2.fileFormId);
            });
        }
    }
}
