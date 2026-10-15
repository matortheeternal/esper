using esper.data;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public abstract record ValueData {
        public sealed record Bytes(byte[] Raw) : ValueData {
            public override long ToInt64() => throw CannotConvert("Int64");
            public override ulong ToUInt64() => throw CannotConvert("UInt64");
            public override float ToSingle() => throw CannotConvert("Single");
            public override object ToPrimitive() => Raw;
        }

        public sealed record Text(string V) : ValueData {
            public override long ToInt64() => Convert.ToInt64(V);
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => Convert.ToSingle(V);
            public override object ToPrimitive() => V;
        }

        public sealed record LString(LocalizedString V) : ValueData {
            public override long ToInt64() => throw CannotConvert("Int64");
            public override ulong ToUInt64() => throw CannotConvert("UInt64");
            public override float ToSingle() => throw CannotConvert("Single");
            public override object ToPrimitive() => V;
        }

        public sealed record Form(FormId V) : ValueData {
            public override long ToInt64() => throw CannotConvert("Int64");
            public override ulong ToUInt64() => throw CannotConvert("UInt64");
            public override float ToSingle() => throw CannotConvert("Single");
            public override object ToPrimitive() => V;
        }

        public sealed record I8(sbyte V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => unchecked((ulong)V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => new[] { unchecked((byte)V) };
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U8(byte V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => V;
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => new[] { V };
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record I16(short V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => unchecked((ulong)V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => new[] { (byte)V, (byte)(V >> 8) };
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U16(ushort V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => V;
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record I32(int V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => unchecked((ulong)V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U32(uint V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => V;
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record I64(long V) : ValueData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => unchecked((ulong)V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U64(ulong V) : ValueData {
            public override long ToInt64() => Convert.ToInt64(V);
            public override ulong ToUInt64() => V;
            public override float ToSingle() => Convert.ToSingle(V);
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record F32(float V) : ValueData {
            public override long ToInt64() => Convert.ToInt64(V);
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
        }

        public abstract long ToInt64();
        public abstract ulong ToUInt64();
        public abstract float ToSingle();
        public abstract object ToPrimitive();

        public sbyte ToSByte() => (sbyte)ToInt64();
        public byte ToByte() => (byte)ToUInt64();
        public short ToInt16() => (short)ToInt64();
        public ushort ToUInt16() => (ushort)ToUInt64();
        public int ToInt32() => (int)ToInt64();
        public uint ToUInt32() => (uint)ToUInt64();

        public virtual byte[] ToBytes() => throw CannotConvert("bytes");

        public virtual string ToHex(string format) => throw CannotConvert("hex sort key");

        public string ToDisplayString() => ToPrimitive().ToString();

        public JToken ToJToken() {
            var primitive = ToPrimitive();
            if (primitive is FormId or LocalizedString)
                throw CannotConvert("JSON");
            return JToken.FromObject(primitive);
        }

        public static implicit operator ValueData(byte[] v) => new Bytes(v);
        public static implicit operator ValueData(string v) => new Text(v);
        public static implicit operator ValueData(LocalizedString v) => new LString(v);
        public static implicit operator ValueData(FormId v) => new Form(v);
        public static implicit operator ValueData(sbyte v) => new I8(v);
        public static implicit operator ValueData(byte v) => new U8(v);
        public static implicit operator ValueData(short v) => new I16(v);
        public static implicit operator ValueData(ushort v) => new U16(v);
        public static implicit operator ValueData(int v) => new I32(v);
        public static implicit operator ValueData(uint v) => new U32(v);
        public static implicit operator ValueData(long v) => new I64(v);
        public static implicit operator ValueData(ulong v) => new U64(v);
        public static implicit operator ValueData(float v) => new F32(v);

        private static readonly Dictionary<Type, Func<object, ValueData>> factories = new() {
            [typeof(byte[])] = v => new Bytes((byte[])v),
            [typeof(string)] = v => new Text((string)v),
            [typeof(LocalizedString)] = v => new LString((LocalizedString)v),
            [typeof(FormId)] = v => new Form((FormId)v),
            [typeof(sbyte)] = v => new I8((sbyte)v),
            [typeof(byte)] = v => new U8((byte)v),
            [typeof(short)] = v => new I16((short)v),
            [typeof(ushort)] = v => new U16((ushort)v),
            [typeof(int)] = v => new I32((int)v),
            [typeof(uint)] = v => new U32((uint)v),
            [typeof(long)] = v => new I64((long)v),
            [typeof(ulong)] = v => new U64((ulong)v),
            [typeof(float)] = v => new F32((float)v),
        };

        public static ValueData From(object value) {
            if (value == null) return null;
            if (value is ValueData v) return v;
            if (factories.TryGetValue(value.GetType(), out var make)) return make(value);
            throw new InvalidOperationException(
                $"Cannot wrap {value.GetType().Name} as value data.");
        }

        private static readonly Dictionary<Type, Func<ValueData, object>> readers = new() {
            [typeof(sbyte)] = d => d.ToSByte(),
            [typeof(byte)] = d => d.ToByte(),
            [typeof(short)] = d => d.ToInt16(),
            [typeof(ushort)] = d => d.ToUInt16(),
            [typeof(int)] = d => d.ToInt32(),
            [typeof(uint)] = d => d.ToUInt32(),
            [typeof(long)] = d => d.ToInt64(),
            [typeof(ulong)] = d => d.ToUInt64(),
            [typeof(float)] = d => d.ToSingle(),
            [typeof(double)] = d => (double)d.ToSingle(),
            [typeof(string)] = d => {
                if (d is Text t) return t.V;
                throw d.CannotConvert("string");
            },
            [typeof(byte[])] = d => {
                if (d is Bytes b) return b.Raw;
                throw d.CannotConvert("byte[]");
            },
            [typeof(FormId)] = d => {
                if (d is Form f) return f.V;
                throw d.CannotConvert("FormId");
            },
            [typeof(LocalizedString)] = d => {
                if (d is LString l) return l.V;
                throw d.CannotConvert("LocalizedString");
            },
            [typeof(ValueData)] = d => d,
            [typeof(object)] = d => d.ToPrimitive(),
        };

        public T As<T>() {
            var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (readers.TryGetValue(target, out var read)) return (T)read(this);
            throw new InvalidOperationException(
                $"Cannot convert value data to {target.Name}.");
        }

        private InvalidOperationException CannotConvert(string target) =>
            new($"Cannot convert {GetType().Name} to {target}.");
    }
}
