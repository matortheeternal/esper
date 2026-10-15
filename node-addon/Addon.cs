using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static unsafe partial class Addon {
    internal enum Status { Ok = 0 }

    [UnmanagedCallersOnly(
        EntryPoint = "napi_register_module_v1",
        CallConvs = [typeof(CallConvCdecl)])]
    public static nint Init(nint env, nint exports) {
        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(), ResolveNode);
        Api.Register(env, exports);
        return exports;
    }

    private static nint ResolveNode(
        string libraryName, Assembly assembly, DllImportSearchPath? searchPath
    ) {
        if (libraryName is not "node") return 0;
        return NativeLibrary.GetMainProgramHandle();
    }

    internal static void RegisterFunction(
        nint env, nint exports, string name,
        delegate* unmanaged[Cdecl]<nint, nint, nint> callback
    ) {
        var bytes = Encoding.UTF8.GetBytes(name);
        fixed (byte* p = bytes) {
            NApi.CreateFunction(env, p, (nuint)bytes.Length, callback, 0, out var fn);
            NApi.SetNamedProperty(env, exports, p, fn);
        }
    }
    internal static string GetString(nint env, nint value) {
        NApi.GetValueStringUtf8(env, value, null, 0, out var len);
        byte[] rented = null;
        Span<byte> buf = (int)len <= 512
            ? stackalloc byte[(int)len + 1]
            : (rented = ArrayPool<byte>.Shared.Rent((int)len + 1));
        try {
            fixed (byte* p = buf)
                NApi.GetValueStringUtf8(env, value, p, len + 1, out _);
            return Encoding.UTF8.GetString(buf[..(int)len]);
        } finally {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    internal static double GetNumber(nint env, nint value) {
        NApi.GetValueDouble(env, value, out var result);
        return result;
    }

    internal static ulong GetBigInt(nint env, nint value) {
        NApi.GetValueBigintUint64(env, value, out var result, out _);
        return result;
    }

    internal static nint JsString(nint env, string value) {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        byte[] rented = null;
        Span<byte> buf = byteCount <= 512
            ? stackalloc byte[byteCount]
            : (rented = ArrayPool<byte>.Shared.Rent(byteCount));
        try {
            Encoding.UTF8.GetBytes(value, buf);
            fixed (byte* p = buf) {
                NApi.CreateStringUtf8(env, p, (nuint)byteCount, out var result);
                return result;
            }
        } finally {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    internal static nint JsNumber(nint env, double value) {
        NApi.CreateDouble(env, value, out var result);
        return result;
    }

    internal static nint JsInt(nint env, long value) {
        NApi.CreateInt64(env, value, out var result);
        return result;
    }

    internal static nint JsBigInt(nint env, ulong value) {
        NApi.CreateBigintUint64(env, value, out var result);
        return result;
    }

    internal static nint JsBool(nint env, bool value) {
        NApi.GetBoolean(env, value ? (byte)1 : (byte)0, out var result);
        return result;
    }

    internal static void Throw(nint env, string message) {
        var byteCount = Encoding.UTF8.GetByteCount(message);
        byte[] rented = null;
        Span<byte> buf = byteCount < 512
            ? stackalloc byte[byteCount + 1]
            : (rented = ArrayPool<byte>.Shared.Rent(byteCount + 1));
        try {
            Encoding.UTF8.GetBytes(message, buf);
            buf[byteCount] = 0;
            fixed (byte* p = buf)
                NApi.ThrowError(env, null, p);
        } finally {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    internal static partial class NApi {
        [LibraryImport("node", EntryPoint = "napi_create_function")]
        internal static partial Status CreateFunction(
            nint env, byte* utf8name, nuint length,
            delegate* unmanaged[Cdecl]<nint, nint, nint> cb,
            nint data, out nint result);

        [LibraryImport("node", EntryPoint = "napi_set_named_property")]
        internal static partial Status SetNamedProperty(
            nint env, nint obj, byte* utf8name, nint value);

        [LibraryImport("node", EntryPoint = "napi_get_cb_info")]
        internal static partial Status GetCallbackInfo(
            nint env, nint cbinfo, nuint* argc,
            nint* argv, nint* thisArg, nint* data);

        [LibraryImport("node", EntryPoint = "napi_get_value_string_utf8")]
        internal static partial Status GetValueStringUtf8(
            nint env, nint value, byte* buf, nuint bufsize, out nuint result);

        [LibraryImport("node", EntryPoint = "napi_create_string_utf8")]
        internal static partial Status CreateStringUtf8(
            nint env, byte* str, nuint length, out nint result);

        [LibraryImport("node", EntryPoint = "napi_get_value_double")]
        internal static partial Status GetValueDouble(
            nint env, nint value, out double result);

        [LibraryImport("node", EntryPoint = "napi_create_double")]
        internal static partial Status CreateDouble(
            nint env, double value, out nint result);

        [LibraryImport("node", EntryPoint = "napi_create_int64")]
        internal static partial Status CreateInt64(
            nint env, long value, out nint result);

        [LibraryImport("node", EntryPoint = "napi_create_bigint_uint64")]
        internal static partial Status CreateBigintUint64(
            nint env, ulong value, out nint result);

        [LibraryImport("node", EntryPoint = "napi_get_value_bigint_uint64")]
        internal static partial Status GetValueBigintUint64(
            nint env, nint value, out ulong result, out byte lossless);

        [LibraryImport("node", EntryPoint = "napi_get_boolean")]
        internal static partial Status GetBoolean(
            nint env, byte value, out nint result);

        [LibraryImport("node", EntryPoint = "napi_throw_error")]
        internal static partial Status ThrowError(
            nint env, byte* code, byte* msg);
    }
}
