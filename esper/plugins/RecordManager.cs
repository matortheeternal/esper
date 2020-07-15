using esper.elements;
using System;

namespace esper.plugins {
    public interface IRecordManager {
        internal RecordMap<ulong> localRecordsByFormId { get; set; }
        internal PluginRecordMap<ulong> remoteRecordsByFormId { get; set; }
        internal PluginFile file { get; }
    }

    public static class RecordManagerExtensions {
        public static void InitRecordMaps(this IRecordManager m) {
            Func<MainRecord, ulong> GetLocalFormId = rec => rec.localFormId;
            m.localRecordsByFormId = new RecordMap<ulong>(GetLocalFormId);
            m.remoteRecordsByFormId = new PluginRecordMap<ulong>(GetLocalFormId);
        }

        public static MainRecord GetRecordByFormId(
            this IRecordManager m, ulong formId
        ) {
            byte ordinal = (byte) (formId >> 24);
            PluginFile targetFile = m.file.OrdinalToFile(ordinal, true);
            ulong localFormId = formId & 0xFFFFFF;
            if (targetFile == m.file) 
                return m.GetLocalRecordByFormId(localFormId);
            var map = m.remoteRecordsByFormId[targetFile];
            return map[localFormId];
        }

        public static MainRecord GetLocalRecordByFormId(
            this IRecordManager m, ulong localFormId
        ) {
            return m.localRecordsByFormId[localFormId];
        }

        public static void IndexRecord(this IRecordManager m, MainRecord rec) {
            if (rec.IsLocal()) {
                m.localRecordsByFormId.Add(rec);
            } else {
                m.remoteRecordsByFormId.Add(rec);
            }
        }
    }
}
