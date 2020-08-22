using esper.elements;
using esper.helpers;
using System;
using System.Collections.Generic;

namespace esper.plugins {
    public interface IRecordManager {
        public List<MainRecord> records { get; set; }
        internal PluginFile file { get; }
    }

    public static class RecordManagerExtensions {
        public static void InitRecordMaps(this IRecordManager m) {
            m.records = new List<MainRecord>((int)m.file.recordCount);
        }

        public static MainRecord GetRecordByFormId(
            this IRecordManager m, UInt32 formId
        ) {
            if (m.file.isDummy) return null;
            return CollectionHelpers.BinarySearch(m.records, rec => {
                return formId.CompareTo(rec.formId);
            });
        }

        public static MainRecord GetRecordByLocalFormId(
            this IRecordManager m, UInt32 localFormId
        ) {
            if (m.file.isDummy) return null;
            var targetOrdinal = m.file.FileToOrdinal(m.file, false);
            return CollectionHelpers.BinarySearch(m.records, rec => {
                var ordinalComparison = targetOrdinal.CompareTo(rec.formId >> 24);
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
                return rec.formId > 0xFFFFFF ? -1 : 1;
            }, true);
            return highRecord.formId > 0xFFFFFF ? 0x800 : highRecord.formId;
        }

        public static void SortRecords(this IRecordManager m) {
            m.records.Sort((rec1, rec2) => {
                return rec1.formId.CompareTo(rec2.formId);
            });
        }
    }
}
