using esper.elements;
using System;
using System.Linq;

namespace esper.plugins {
    public interface IRecordManager {
        internal FormIdMap localRecordsByFormId { get; set; }
        internal PluginRecordMap remoteRecordsByFormId { get; set; }
        internal PluginFile file { get; }
    }

    public static class RecordManagerExtensions {
        public static void InitRecordMaps(this IRecordManager m) {
            m.localRecordsByFormId = new FormIdMap();
            m.remoteRecordsByFormId = new PluginRecordMap();
        }

        public static MainRecord GetRecordByFormId(
            this IRecordManager m, UInt32 formId
        ) {
            byte ordinal = (byte) (formId >> 24);
            PluginFile targetFile = m.file.OrdinalToFile(ordinal, true);
            UInt32 localFormId = formId & 0xFFFFFF;
            if (targetFile == m.file) 
                return m.GetLocalRecordByFormId(localFormId);
            var map = m.remoteRecordsByFormId.GetMap(targetFile);
            return map.Get(localFormId);
        }

        public static MainRecord GetLocalRecordByFormId(
            this IRecordManager m, UInt32 localFormId
        ) {
            return m.localRecordsByFormId.Get(localFormId);
        }

        public static void IndexRecord(this IRecordManager m, MainRecord rec) {
            var ord = rec.formId >> 24;
            var file = (m.file as IMasterManager);
            var newOrd = file.originalMasters.Count;
            if (ord >= newOrd) {
                m.localRecordsByFormId.Add(rec);
            } else {
                var plugin = file.OrdinalToFile((byte) ord, false);
                m.remoteRecordsByFormId.Add(plugin, rec);
            }
        }

        public static UInt32 GetHighObjectID(this IRecordManager m) {
            if (m.localRecordsByFormId == null) return 0;
            return m.localRecordsByFormId.highObjectId;
        }
    }
}
