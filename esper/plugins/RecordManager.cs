using esper.elements;

namespace esper.plugins {
    public interface IRecordManager {
        internal RecordMap<ulong> localRecordsByFormID { get; set; }
        internal PluginRecordMap<ulong> remoteRecordsByFormId { get; set; }
        internal PluginFile file { get; }
    }

    public static class RecordManagerExtensions {
        public static MainRecord GetRecordByFormId(this IRecordManager m, ulong formId) {
            byte ordinal = (byte) (formId >> 24);
            PluginFile targetFile = m.file.OrdinalToFile(ordinal, true);
            var map = m.remoteRecordsByFormId[targetFile];
            ulong localFormId = formId & 0xFFFFFF;
            return map[localFormId];
        }

        public static MainRecord GetLocalRecordByFormId(this IRecordManager m, ulong localFormId) {
            return m.localRecordsByFormID[localFormId];
        }
    }
}
