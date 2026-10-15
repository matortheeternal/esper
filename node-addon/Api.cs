using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using esper.elements;
using esper.plugins;
using esper.setup;

internal static unsafe class Api {
    internal static void Register(nint env, nint exports) {
        Addon.RegisterFunction(env, exports, "detectGames", &DetectGames);
        Addon.RegisterFunction(env, exports, "openSession", &OpenSession);
        Addon.RegisterFunction(env, exports, "free", &Free);
        Addon.RegisterFunction(env, exports, "loadOrder", &LoadOrder);
        Addon.RegisterFunction(env, exports, "loadPlugin", &LoadPlugin);
        Addon.RegisterFunction(env, exports, "pluginName", &PluginName);
        Addon.RegisterFunction(env, exports, "pluginPath", &PluginPath);
        Addon.RegisterFunction(env, exports, "pluginRecordCount", &PluginRecordCount);
        Addon.RegisterFunction(env, exports, "pluginMasters", &PluginMasters);
        Addon.RegisterFunction(env, exports, "pluginLocalized", &PluginLocalized);
        Addon.RegisterFunction(env, exports, "pluginModified", &PluginModified);
        Addon.RegisterFunction(env, exports, "associatedArchives", &AssociatedArchives);
        Addon.RegisterFunction(env, exports, "getRecord", &GetRecord);
        Addon.RegisterFunction(env, exports, "recordFormId", &RecordFormId);
        Addon.RegisterFunction(env, exports, "recordEditorId", &RecordEditorId);
        Addon.RegisterFunction(env, exports, "recordSignature", &RecordSignature);
        Addon.RegisterFunction(env, exports, "childCount", &ChildCount);
        Addon.RegisterFunction(env, exports, "getChild", &GetChild);
        Addon.RegisterFunction(env, exports, "elementKind", &ElementKind);
        Addon.RegisterFunction(env, exports, "elementLabel", &ElementLabel);
        Addon.RegisterFunction(env, exports, "elementValue", &ElementValue);
        Addon.RegisterFunction(env, exports, "savePlugin", &SavePlugin);
    }

    private static ulong Pin(object target) =>
        (ulong)GCHandle.ToIntPtr(GCHandle.Alloc(target));

    private static T Unpin<T>(ulong handle) where T : class =>
        (T)GCHandle.FromIntPtr((nint)handle).Target;

    private static nint Run(
        nint env, nint info, Func<nint, nint[], nint> body
    ) {
        try {
            nuint argc = 2;
            var argv = stackalloc nint[2];
            Addon.NApi.GetCallbackInfo(env, info, &argc, argv, null, null);
            var args = new nint[argc];
            for (var i = 0; i < args.Length; i++)
                args[i] = argv[i];
            return body(env, args);
        } catch (Exception ex) {
            Addon.Throw(env, ex.Message);
            return 0;
        }
    }

    private static Session SessionOf(nint env, nint[] args) =>
        Unpin<Session>(Addon.GetBigInt(env, args[0]));

    private static PluginFile PluginOf(nint env, nint[] args, int at = 0) =>
        Unpin<PluginFile>(Addon.GetBigInt(env, args[at]));

    private static Element ElementOf(nint env, nint[] args, int at = 0) =>
        Unpin<Element>(Addon.GetBigInt(env, args[at]));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint DetectGames(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsString(env, Backend.DetectGames()));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint OpenSession(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBigInt(env, Pin(Backend.OpenSession(
            Addon.GetString(env, args[0]), Addon.GetString(env, args[1])))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint Free(nint env, nint info) =>
        Run(env, info, (env, args) => {
            GCHandle.FromIntPtr((nint)Addon.GetBigInt(env, args[0])).Free();
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint LoadOrder(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsString(env, Backend.LoadOrder(SessionOf(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint LoadPlugin(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var session = SessionOf(env, args);
            var full = Addon.GetString(env, args[1]);
            if (!Path.IsPathRooted(full))
                full = Path.Combine(session.dataPath, full);
            return Addon.JsBigInt(env, Pin(session.pluginManager.LoadPlugin(full)));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint PluginName(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsString(env, PluginOf(env, args).name));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint PluginPath(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsString(env, PluginOf(env, args).filePath ?? ""));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint PluginRecordCount(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsInt(env, PluginOf(env, args).records?.Count ?? 0));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint PluginMasters(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsString(env, Json.List(PluginOf(env, args).masters.filenames)));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint PluginLocalized(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsBool(env, Backend.SafeFlag(() => PluginOf(env, args).localized)));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint PluginModified(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsBool(env, Backend.IsModified(PluginOf(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint AssociatedArchives(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsString(env, Json.List(
            SessionOf(env, args).resourceManager.GetAssociatedArchives(
                Addon.GetString(env, args[1])))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetRecord(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var plugin = PluginOf(env, args);
            var index = (int)Addon.GetNumber(env, args[1]);
            if (index < 0 || index >= plugin.records.Count)
                throw new Exception($"record index {index} out of range");
            return Addon.JsBigInt(env, Pin(plugin.records[index]));
        });

    private static MainRecord AsRecord(nint env, nint[] args) {
        if (ElementOf(env, args) is MainRecord rec) return rec;
        throw new Exception("not a record");
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint RecordFormId(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsNumber(env, AsRecord(env, args).fileFormId));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint RecordEditorId(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var rec = AsRecord(env, args);
            return Addon.JsString(env, Backend.Safe(() => rec.editorId));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint RecordSignature(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var rec = AsRecord(env, args);
            return Addon.JsString(env, Backend.CleanSig(
                Backend.Safe(() => rec.signature.ToString())));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint ChildCount(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsInt(env,
            ElementOf(env, args) is Container c ? c.count : 0));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetChild(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not Container c)
                throw new Exception("element has no children");
            var index = (int)Addon.GetNumber(env, args[1]);
            if (index < 0 || index >= c.count)
                throw new Exception($"child index {index} out of range");
            return Addon.JsBigInt(env, Pin(c.elements[index]));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint ElementKind(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsString(env, Backend.KindOf(ElementOf(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint ElementLabel(nint env, nint info) =>
        Run(env, info, (env, args) =>
            Addon.JsString(env, Backend.LabelOf(ElementOf(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint ElementValue(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsString(env,
            ElementOf(env, args) is ValueElement v
                ? Backend.Safe(() => v.value) : ""));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SavePlugin(nint env, nint info) =>
        Run(env, info, (env, args) => {
            PluginOf(env, args).Save(Addon.GetString(env, args[1]));
            return Addon.JsBool(env, true);
        });
}
