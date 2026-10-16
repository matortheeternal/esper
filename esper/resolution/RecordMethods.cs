using esper.elements;

namespace esper.resolution {
    public interface IMainRecord { };

    //[JSExport]
    public static class MainRecordExtensions {
        public static bool GetRecordFlag(this MainRecord m, string flag) {
            if (m == null) return false;
            return m.mrDef.RecordFlagIsSet(m, flag);
        }

        public static bool GetRecordFlag(this MainRecord m, int flagIndex) {
            if (m == null) return false;
            return m.mrDef.RecordFlagIsSet(m, flagIndex);
        }

        public static void SetRecordFlag(this MainRecord m, int flagIndex, bool state) {
            if (m == null) return;
            uint bit = (uint)1 << flagIndex;
            var header = m.header;
            if (state) header.flags |= bit;
            else header.flags &= ~bit;
            m.header = header;
            if (m.GetElement(@"Record Header\Record Flags") is ValueElement ve)
                ve.data = (ulong)m.header.flags;
            m.MarkModified();
        }
    }
}
