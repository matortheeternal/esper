using esper.elements;

namespace esper.resolution {
    public interface IMainRecord {};

    [JSExport]
    public static class MainRecordExtensions {
        public static bool GetRecordFlag(this MainRecord m, string flag) {
            return m.mrDef.RecordFlagIsSet(m, flag);
        }

        public static bool GetRecordFlag(this MainRecord m, int flagIndex) {
            return m.mrDef.RecordFlagIsSet(m, flagIndex);
        }
    }
}
