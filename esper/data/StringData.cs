using esper.parsing;

namespace esper.data {
    public class StringData : DataContainer {
        public readonly string data;
        public int size {
            get => data.Length;
        }

        public StringData(PluginFileSource source, int size) {
            data = source.ReadString(size);
        }

        public override string ToString() {
            return data;
        }
    }
}
