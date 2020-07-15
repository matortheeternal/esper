using esper.parsing;

namespace esper.data {
    public class StringData : DataContainer {
        public readonly string data;
        public int size { get => data.Length; }

        public StringData(string data) {
            this.data = data;
        }

        public override string ToString() {
            return data;
        }
    }
}
