using esper.helpers;
using esper.parsing;

namespace esper.data {
    public class BytesData : DataContainer {
        public readonly byte[] data;
        public int size {
            get => data.Length;
        }

        public BytesData(byte[] data) {
            this.data = data;
        }

        public override string ToString() {
            return StringHelpers.FormatBytes(data);
        }
    }
}
