using esper.elements;
using System.Collections.Generic;

namespace esper.plugins {
    public class RecordMap<T> : Dictionary<T, MainRecord> {}

    public class PluginRecordMap<T> : Dictionary<PluginFile, RecordMap<T>> {}
}
