using esper.elements;
using System;
using System.Linq;

namespace esper.plugins {
    public interface IRecordManager {
        internal RecordMap<UInt32> localRecordsByFormId { get; set; }
        internal PluginRecordMap<UInt32> remoteRecordsByFormId { get; set; }
        internal PluginFile file { get; }
    }

    public static class RecordManagerExtensions {
        public static void InitRecordMaps(this IRecordManager m) {
            static UInt32 GetLocalFormId(MainRecord rec) => rec.localFormId;
            m.localRecordsByFormId = new RecordMap<UInt32>(GetLocalFormId);
            m.remoteRecordsByFormId = new PluginRecordMap<UInt32>(GetLocalFormId);
        }

        public static MainRecord GetRecordByFormId(
            this IRecordManager m, UInt32 formId
        ) {
            byte ordinal = (byte) (formId >> 24);
            PluginFile targetFile = m.file.OrdinalToFile(ordinal, true);
            UInt32 localFormId = formId & 0xFFFFFF;
            if (targetFile == m.file) 
                return m.GetLocalRecordByFormId(localFormId);
            var map = m.remoteRecordsByFormId[targetFile];
            return map[localFormId];
        }

        public static MainRecord GetLocalRecordByFormId(
            this IRecordManager m, UInt32 localFormId
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

        public static UInt32 GetHighObjectID(this IRecordManager m) {
            if (m.localRecordsByFormId == null) return 0;
            return m.localRecordsByFormId.Last().Key;
        }
    }
}
