﻿using esper.elements;

namespace esper.resolution {
    public interface IMainRecord {};

    public static class MainRecordExtensions {
        public static bool GetRecordFlag(this MainRecord m, string flag) {
            return m.mrDef.RecordFlagIsSet(m, flag);
        }
    }
}
