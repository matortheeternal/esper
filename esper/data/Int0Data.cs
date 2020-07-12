namespace esper.data {
    public class Int0Data : DataContainer {
        public static string defType = "int0";
        public byte data { get => 0; }
        public int size { get => 0; }

        public Int0Data() {}

        public override string ToString() {
            return "0";
        }
    }
}
