using esper.parsing;
using System;

namespace esper.data {
    public class IntData<T> : DataContainer {
        public readonly static Func<PluginFileSource, T> Read;

        static IntData() {
            if (typeof(T) == typeof(byte)) {
                Read = (PluginFileSource source) => (T)(object)source.reader.ReadByte();
            } else if (typeof(T) == typeof(UInt16)) {
                Read = (PluginFileSource source) => (T)(object)source.reader.ReadUInt16();
            } else if (typeof(T) == typeof(UInt32)) {
                Read = (PluginFileSource source) => (T)(object)source.reader.ReadUInt32();
            } else if (typeof(T) == typeof(sbyte)) {
                Read = (PluginFileSource source) => (T)(object)source.reader.ReadSByte();
            } else if (typeof(T) == typeof(Int16)) {
                Read = (PluginFileSource source) => (T)(object)source.reader.ReadInt16();
            } else if (typeof(T) == typeof(Int32)) {
                Read = (PluginFileSource source) => (T)(object)source.reader.ReadInt32();
            } else {
                throw new Exception("Unsupported IntData type");
            }
        }

        public T data;
        public IntData(T data) {
            this.data = data;
        }
        public IntData(PluginFileSource source) {
            data = Read(source);
        }

        public override string ToString() {
            return data.ToString();
        }
    }
}
