using esper.data;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public abstract record VariantData {
        public sealed record Bytes(byte[] Raw) : VariantData {
            public override long ToInt64() => throw CannotConvert("Int64");
            public override ulong ToUInt64() => throw CannotConvert("UInt64");
            public override float ToSingle() => throw CannotConvert("Single");
            public override object ToPrimitive() => Raw;
        }

        public sealed record Text(string V) : VariantData {
            public override long ToInt64() => Convert.ToInt64(V);
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => Convert.ToSingle(V);
            public override object ToPrimitive() => V;
        }

        public sealed record LString(LocalizedString V) : VariantData {
            public override long ToInt64() => throw CannotConvert("Int64");
            public override ulong ToUInt64() => throw CannotConvert("UInt64");
            public override float ToSingle() => throw CannotConvert("Single");
            public override object ToPrimitive() => V;
        }

        public sealed record Form(FormId V) : VariantData {
            public override long ToInt64() => throw CannotConvert("Int64");
            public override ulong ToUInt64() => throw CannotConvert("UInt64");
            public override float ToSingle() => throw CannotConvert("Single");
            public override object ToPrimitive() => V;
        }

        public sealed record I8(sbyte V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => new[] { unchecked((byte)V) };
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U8(byte V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => V;
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => new[] { V };
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record I16(short V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => new[] { (byte)V, (byte)(V >> 8) };
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U16(ushort V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => V;
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record I32(int V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U32(uint V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => V;
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record I64(long V) : VariantData {
            public override long ToInt64() => V;
            public override ulong ToUInt64() => Convert.ToUInt64(V);
            public override float ToSingle() => V;
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record U64(ulong V) : VariantData {
            public override long ToInt64() => Convert.ToInt64(V);
            public override ulong ToUInt64() => V;
            public override float ToSingle() => Convert.ToSingle(V);
            public override object ToPrimitive() => V;
            public override byte[] ToBytes() => BitConverter.GetBytes(V);
            public override string ToHex(string format) => V.ToString(format);
        }

        public sealed record F32(float V) : VariantData {
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

        public sbyte ToSByte() => Convert.ToSByte(ToInt64());
        public byte ToByte() => Convert.ToByte(ToInt64());
        public short ToInt16() => Convert.ToInt16(ToInt64());
        public ushort ToUInt16() => Convert.ToUInt16(ToInt64());
        public int ToInt32() => Convert.ToInt32(ToInt64());
        public uint ToUInt32() => Convert.ToUInt32(ToInt64());

        public virtual byte[] ToBytes() => throw CannotConvert("bytes");

        public virtual string ToHex(string format) => throw CannotConvert("hex sort key");

        public string ToDisplayString() => ToPrimitive().ToString();

        public JToken ToJToken() {
            var primitive = ToPrimitive();
            if (primitive is FormId or LocalizedString)
                throw CannotConvert("JSON");
            return JToken.FromObject(primitive);
        }

        public static implicit operator VariantData(byte[] v) => new Bytes(v);
        public static implicit operator VariantData(string v) => new Text(v);
        public static implicit operator VariantData(LocalizedString v) => new LString(v);
        public static implicit operator VariantData(FormId v) => new Form(v);
        public static implicit operator VariantData(sbyte v) => new I8(v);
        public static implicit operator VariantData(byte v) => new U8(v);
        public static implicit operator VariantData(short v) => new I16(v);
        public static implicit operator VariantData(ushort v) => new U16(v);
        public static implicit operator VariantData(int v) => new I32(v);
        public static implicit operator VariantData(uint v) => new U32(v);
        public static implicit operator VariantData(long v) => new I64(v);
        public static implicit operator VariantData(ulong v) => new U64(v);
        public static implicit operator VariantData(float v) => new F32(v);

        private static readonly Dictionary<Type, Func<object, VariantData>> factories = new() {
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

        public static VariantData From(object value) {
            if (value == null) return null;
            if (value is VariantData v) return v;
            if (factories.TryGetValue(value.GetType(), out var make)) return make(value);
            throw new InvalidOperationException(
                $"Cannot wrap {value.GetType().Name} as value data.");
        }

        private static readonly Dictionary<Type, Func<VariantData, object>> readers = new() {
            [typeof(sbyte)] = d => d.ToSByte(),
            [typeof(byte)] = d => d.ToByte(),
            [typeof(short)] = d => d.ToInt16(),
            [typeof(ushort)] = d => d.ToUInt16(),
            [typeof(int)] = d => d.ToInt32(),
            [typeof(uint)] = d => d.ToUInt32(),
            [typeof(long)] = d => d.ToInt64(),
            [typeof(ulong)] = d => d.ToUInt64(),
            [typeof(float)] = d => d.ToSingle(),
            [typeof(double)] = d => d is F32 f ? f.V : Convert.ToDouble(d.ToInt64()),
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
            [typeof(VariantData)] = d => d,
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
