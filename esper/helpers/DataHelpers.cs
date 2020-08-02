using System;
using System.Text.RegularExpressions;

namespace esper.helpers {
    public static class DataHelpers {
        private static Regex numberExpr = new Regex(@"^(-?\d+)");

        public static sbyte ClampToInt8(Int64 data) {
            if (data > sbyte.MaxValue) return sbyte.MaxValue;
            if (data < sbyte.MinValue) return sbyte.MinValue;
            return (sbyte)data;
        }

        public static Int16 ClampToInt16(Int64 data) {
            if (data > Int16.MaxValue) return Int16.MaxValue;
            if (data < Int16.MinValue) return Int16.MinValue;
            return (Int16)data;
        }

        public static Int64 ParseInt64(string value, Int64 defaultValue = -1) {
            var match = numberExpr.Match(value);
            if (match == null) return defaultValue;
            return Int64.Parse(match.Groups[1].Value);
        }

        public static Int32 ClampToInt32(Int64 data) {
            if (data > Int32.MaxValue) return Int32.MaxValue;
            if (data < Int32.MinValue) return Int32.MinValue;
            return (Int32)data;
        }

        public static byte ClampToUInt8(Int64 data) {
            if (data > byte.MaxValue) return byte.MaxValue;
            if (data < byte.MinValue) return byte.MinValue;
            return (byte)data;
        }

        public static UInt16 ClampToUInt16(Int64 data) {
            if (data > UInt16.MaxValue) return UInt16.MaxValue;
            if (data < UInt16.MinValue) return UInt16.MinValue;
            return (UInt16)data;
        }

        public static UInt32 ClampToUInt32(Int64 data) {
            if (data > UInt32.MaxValue) return UInt32.MaxValue;
            if (data < UInt32.MinValue) return UInt32.MinValue;
            return (UInt32)data;
        }
    }
}
