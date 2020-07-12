using esper.helpers;
using esper.parsing;

namespace esper.data {
    public class BytesData : DataContainer {
        public readonly byte[] data;
        public int size {
            get => data.Length;
        }

        public BytesData(PluginFileSource source, int size) {
            data = source.reader.ReadBytes(size);
        }

        public override string ToString() {
            return StringHelpers.FormatBytes(data);
        }
    }
}
