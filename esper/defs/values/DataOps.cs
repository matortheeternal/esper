using esper.data;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public static class DataOps {
        public static long ToInt64(this object data) => data switch {
            sbyte v => v,
            byte v => v,
            short v => v,
            ushort v => v,
            int v => v,
            uint v => v,
            long v => v,
            ulong v => Convert.ToInt64(v),
            float v => Convert.ToInt64(v),
            double v => Convert.ToInt64(v),
            string v => Convert.ToInt64(v),
            _ => throw new InvalidOperationException(
                $"Cannot convert {data?.GetType().Name} to Int64."),
        };

        public static ulong ToUInt64(this object data) => data switch {
            sbyte v => unchecked((ulong)v),
            byte v => v,
            short v => unchecked((ulong)v),
            ushort v => v,
            int v => unchecked((ulong)v),
            uint v => v,
            long v => unchecked((ulong)v),
            ulong v => v,
            float v => Convert.ToUInt64(v),
            double v => Convert.ToUInt64(v),
            string v => Convert.ToUInt64(v),
            _ => throw new InvalidOperationException(
                $"Cannot convert {data?.GetType().Name} to UInt64."),
        };

        public static float ToSingle(this object data) => data switch {
            sbyte v => v,
            byte v => v,
            short v => v,
            ushort v => v,
            int v => v,
            uint v => v,
            long v => v,
            ulong v => Convert.ToSingle(v),
            float v => v,
            double v => (float)v,
            string v => Convert.ToSingle(v),
            _ => throw new InvalidOperationException(
                $"Cannot convert {data?.GetType().Name} to Single."),
        };

        public static string ToDisplayString(this object data) =>
            data.ToString();

        public static JToken ToJToken(this object data) => data switch {
            byte[] v => JToken.FromObject(v),
            string v => JToken.FromObject(v),
            sbyte v => JToken.FromObject(v),
            byte v => JToken.FromObject(v),
            short v => JToken.FromObject(v),
            ushort v => JToken.FromObject(v),
            int v => JToken.FromObject(v),
            uint v => JToken.FromObject(v),
            long v => JToken.FromObject(v),
            ulong v => JToken.FromObject(v),
            float v => JToken.FromObject(v),
            double v => JToken.FromObject(v),
            _ => throw new InvalidOperationException(
                $"Cannot serialize {data?.GetType().Name} to JSON."),
        };
    }
}
