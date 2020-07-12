using esper.parsing;
using System;

namespace esper.data {
    public class IntData<T> : DataContainer {
        public T data;

        public override string ToString() {
            return data.ToString();
        }
    }

    public class Int32Data : IntData<Int32> {
        public Int32Data(PluginFileSource source) {
            data = source.reader.ReadInt32();
        }
    }

    public class Int16Data : IntData<Int16> {
        public Int16Data(PluginFileSource source) {
            data = source.reader.ReadInt16();
        }
    }

    public class Int8Data : IntData<sbyte> {
        public Int8Data(PluginFileSource source) {
            data = source.reader.ReadSByte();
        }
    }

    public class UInt32Data : IntData<UInt32> {
        public UInt32Data(PluginFileSource source) {
            data = source.reader.ReadUInt32();
        }
    }

    public class UInt16Data : IntData<UInt16> {
        public UInt16Data(PluginFileSource source) {
            data = source.reader.ReadUInt16();
        }
    }

    public class UInt8Data : IntData<byte> {
        public UInt8Data(PluginFileSource source) {
            data = source.reader.ReadByte();
        }
    }
}
