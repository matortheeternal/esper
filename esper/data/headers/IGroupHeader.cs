namespace esper.data.headers {
    [JSExport]
    public interface IGroupHeader {
        public Signature signature { get; }
        public UInt32 groupSize { get; set; }
        public byte[] label { get; }
        public Int32 groupType { get; }
    }
}
