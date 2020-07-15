using esper.parsing;

namespace esper.data {
    public class FloatData : DataContainer {
        public float data;

        public FloatData(PluginFileSource source) {
            data = source.reader.ReadSingle();
        }

        public FloatData(float data) {
            this.data = data;
        }

        public override string ToString() {
            return data.ToString("N5");
        }
    }
}
