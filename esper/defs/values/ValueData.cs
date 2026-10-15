using esper.data;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public abstract record ValueData {
        public sealed record Bytes(byte[] Raw) : ValueData;
        public sealed record Text(string V) : ValueData;
        public sealed record LString(LocalizedString V) : ValueData;
        public sealed record Form(FormId V) : ValueData;
        public sealed record I8(sbyte V) : ValueData;
        public sealed record U8(byte V) : ValueData;
        public sealed record I16(short V) : ValueData;
        public sealed record U16(ushort V) : ValueData;
        public sealed record I32(int V) : ValueData;
        public sealed record U32(uint V) : ValueData;
        public sealed record I64(long V) : ValueData;
        public sealed record U64(ulong V) : ValueData;
        public sealed record F32(float V) : ValueData;

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

        public long ToInt64() => this switch {
            I8 v => v.V,
            U8 v => v.V,
            I16 v => v.V,
            U16 v => v.V,
            I32 v => v.V,
            U32 v => v.V,
            I64 v => v.V,
            U64 v => Convert.ToInt64(v.V),
            F32 v => Convert.ToInt64(v.V),
            Text v => Convert.ToInt64(v.V),
            _ => throw new InvalidOperationException(
                $"Cannot convert {GetType().Name} to Int64."),
        };

        public ulong ToUInt64() => this switch {
            I8 v => unchecked((ulong)v.V),
            U8 v => v.V,
            I16 v => unchecked((ulong)v.V),
            U16 v => v.V,
            I32 v => unchecked((ulong)v.V),
            U32 v => v.V,
            I64 v => unchecked((ulong)v.V),
            U64 v => v.V,
            F32 v => Convert.ToUInt64(v.V),
            Text v => Convert.ToUInt64(v.V),
            _ => throw new InvalidOperationException(
                $"Cannot convert {GetType().Name} to UInt64."),
        };

        public float ToSingle() => this switch {
            I8 v => v.V,
            U8 v => v.V,
            I16 v => v.V,
            U16 v => v.V,
            I32 v => v.V,
            U32 v => v.V,
            I64 v => v.V,
            U64 v => Convert.ToSingle(v.V),
            F32 v => v.V,
            Text v => Convert.ToSingle(v.V),
            _ => throw new InvalidOperationException(
                $"Cannot convert {GetType().Name} to Single."),
        };

        public JToken ToJToken() => this switch {            Bytes v => JToken.FromObject(v.Raw),
            Text v => JToken.FromObject(v.V),
            I8 v => JToken.FromObject(v.V),
            U8 v => JToken.FromObject(v.V),
            I16 v => JToken.FromObject(v.V),
            U16 v => JToken.FromObject(v.V),
            I32 v => JToken.FromObject(v.V),
            U32 v => JToken.FromObject(v.V),
            I64 v => JToken.FromObject(v.V),
            U64 v => JToken.FromObject(v.V),
            F32 v => JToken.FromObject(v.V),
            _ => throw new InvalidOperationException(
                $"Cannot serialize {GetType().Name} to JSON."),
        };

        public string ToDisplayString() => this switch {
            Bytes v => v.Raw.ToString(),
            Text v => v.V,
            LString => throw new InvalidOperationException(
                "Localized strings need an element for display."),
            Form v => v.V.ToString(),
            I8 v => v.V.ToString(),
            U8 v => v.V.ToString(),
            I16 v => v.V.ToString(),
            U16 v => v.V.ToString(),
            I32 v => v.V.ToString(),
            U32 v => v.V.ToString(),
            I64 v => v.V.ToString(),
            U64 v => v.V.ToString(),
            F32 v => v.V.ToString(),
            _ => throw new InvalidOperationException("Unknown value data."),
        };
        public object ToPrimitive() => this switch {
            Bytes v => v.Raw,
            Text v => v.V,
            LString v => v.V,
            Form v => v.V,
            I8 v => v.V,
            U8 v => v.V,
            I16 v => v.V,
            U16 v => v.V,
            I32 v => v.V,
            U32 v => v.V,
            I64 v => v.V,
            U64 v => v.V,
            F32 v => v.V,
            _ => throw new InvalidOperationException("Unknown value data."),
        };

        public static ValueData From(object value) => value switch {
            null => null,
            ValueData v => v,
            byte[] v => new Bytes(v),
            string v => new Text(v),
            LocalizedString v => new LString(v),
            FormId v => new Form(v),
            sbyte v => new I8(v),
            byte v => new U8(v),
            short v => new I16(v),
            ushort v => new U16(v),
            int v => new I32(v),
            uint v => new U32(v),
            long v => new I64(v),
            ulong v => new U64(v),
            float v => new F32(v),
            _ => throw new InvalidOperationException(
                $"Cannot wrap {value.GetType().Name} as value data."),
        };

        public T As<T>() {
            var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object result = target switch {
                var t when t == typeof(sbyte) => (sbyte)ToInt64(),
                var t when t == typeof(byte) => (byte)ToUInt64(),
                var t when t == typeof(short) => (short)ToInt64(),
                var t when t == typeof(ushort) => (ushort)ToUInt64(),
                var t when t == typeof(int) => (int)ToInt64(),
                var t when t == typeof(uint) => (uint)ToUInt64(),
                var t when t == typeof(long) => ToInt64(),
                var t when t == typeof(ulong) => ToUInt64(),
                var t when t == typeof(float) => ToSingle(),
                var t when t == typeof(double) => (double)ToSingle(),
                var t when t == typeof(string) =>
                    this is Text s ? s.V : throw Mismatch("string"),
                var t when t == typeof(byte[]) => this is Bytes b ? b.Raw : throw Mismatch("byte[]"),
                var t when t == typeof(FormId) => this is Form f ? f.V : throw Mismatch("FormId"),
                var t when t == typeof(LocalizedString) =>
                    this is LString l ? l.V : throw Mismatch("LocalizedString"),
                var t when t == typeof(ValueData) => this,
                var t when t == typeof(object) => ToPrimitive(),
                _ => throw new InvalidOperationException(
                    $"Cannot convert value data to {target.Name}."),
            };
            return (T)result;
        }

        private InvalidOperationException Mismatch(string want) =>
            new($"Expected {want}, got {GetType().Name}.");
    }
}
