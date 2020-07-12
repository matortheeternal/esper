using esper.parsing;
using System.Runtime.InteropServices;

namespace esper.data {
    public class FloatData : DataContainer {
        public float data;

        public FloatData(PluginFileSource source) {
            data = source.reader.ReadSingle();
        }

        public override string ToString() {
            return data.ToString("N5");
        }
    }
}
