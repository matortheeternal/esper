using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using esper.data;
using esper.elements;
using esper.plugins;
using esper.resolution;
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
        Addon.RegisterFunction(env, exports, "editorID", &RecordEditorId);
        Addon.RegisterFunction(env, exports, "getElement", &GetElement);
        Addon.RegisterFunction(env, exports, "getElementEx", &GetElementEx);
        Addon.RegisterFunction(env, exports, "hasElement", &HasElement);
        Addon.RegisterFunction(env, exports, "getElements", &GetElements);
        Addon.RegisterFunction(env, exports, "addElement", &AddElement);
        Addon.RegisterFunction(env, exports, "fileByName", &FileByName);
        Addon.RegisterFunction(env, exports, "getRecordByFormId", &GetRecordByFormId);
        Addon.RegisterFunction(env, exports, "getValue", &GetValue);
        Addon.RegisterFunction(env, exports, "setValue", &SetValue);
        Addon.RegisterFunction(env, exports, "getIntValue", &GetIntValue);
        Addon.RegisterFunction(env, exports, "setIntValue", &SetIntValue);
        Addon.RegisterFunction(env, exports, "getUIntValue", &GetUIntValue);
        Addon.RegisterFunction(env, exports, "setUIntValue", &SetUIntValue);
        Addon.RegisterFunction(env, exports, "getFloatValue", &GetFloatValue);
        Addon.RegisterFunction(env, exports, "setFloatValue", &SetFloatValue);
        Addon.RegisterFunction(env, exports, "getSignature", &GetSignature);
        Addon.RegisterFunction(env, exports, "getIsModified", &GetIsModified);
        Addon.RegisterFunction(env, exports, "defType", &DefType);
        Addon.RegisterFunction(env, exports, "getContainer", &GetContainer);
        Addon.RegisterFunction(env, exports, "elementPath", &ElementPath);
        Addon.RegisterFunction(env, exports, "removeElement", &RemoveElement);
        Addon.RegisterFunction(env, exports, "copyElement", &CopyElement);
        Addon.RegisterFunction(env, exports, "exchangeReferences", &ExchangeReferences);
        Addon.RegisterFunction(env, exports, "getRecordFlag", &GetRecordFlag);
        Addon.RegisterFunction(env, exports, "setRecordFlag", &SetRecordFlag);
        Addon.RegisterFunction(env, exports, "getMasterRecord", &GetMasterRecord);
        Addon.RegisterFunction(env, exports, "getWinningOverride", &GetWinningOverride);
        Addon.RegisterFunction(env, exports, "isWinningOverride", &IsWinningOverride);
        Addon.RegisterFunction(env, exports, "getReferencedBy", &GetReferencedBy);
        Addon.RegisterFunction(env, exports, "buildReferences", &BuildReferences);
        Addon.RegisterFunction(env, exports, "getLinksTo", &GetLinksTo);
        Addon.RegisterFunction(env, exports, "addMaster", &AddMaster);
        Addon.RegisterFunction(env, exports, "cleanMasters", &CleanMasters);
        Addon.RegisterFunction(env, exports, "getIsEditable", &GetIsEditable);
        Addon.RegisterFunction(env, exports, "getFlag", &GetElementFlag);
        Addon.RegisterFunction(env, exports, "setFlag", &SetElementFlag);
    }

    private static ulong Pin(object target) =>
        (ulong)GCHandle.ToIntPtr(GCHandle.Alloc(target));

    private static T Unpin<T>(ulong handle) where T : class =>
        (T)GCHandle.FromIntPtr((nint)handle).Target;

    private static nint Run(
        nint env, nint info, Func<nint, nint[], nint> body
    ) {
        try {
            nuint argc = 0;
            Addon.NApi.GetCallbackInfo(env, info, &argc, null, null, null);
            var argv = stackalloc nint[(int)argc];
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

    private static string PathArg(nint env, nint[] args) =>
        args.Length > 1 ? Addon.GetString(env, args[1]) : "";

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
            var plugin = PluginOf(env, args);
            var path = args.Length > 1 ? Addon.GetString(env, args[1]) : "";
            if (string.IsNullOrWhiteSpace(path)) path = plugin.filePath;
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("no path to save to");
            plugin.Save(path);
            return Addon.JsBool(env, true);
        });

    private static nint ResolveOrZero(
        nint env, nint[] args, Func<Element, Element> resolve
    ) {
        var target = resolve(ElementOf(env, args));
        return target == null
            ? Addon.JsBigInt(env, 0)
            : Addon.JsBigInt(env, Pin(target));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetElement(nint env, nint info) =>
        Run(env, info, (env, args) => ResolveOrZero(env, args,
            el => el.GetElement(PathArg(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetElementEx(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBigInt(env, Pin(
            ElementOf(env, args).GetElementEx(PathArg(env, args)))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint HasElement(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBool(env,
            ElementOf(env, args).GetElement(PathArg(env, args)) != null));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetElements(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var el = ElementOf(env, args);
            var path = PathArg(env, args);
            var target = string.IsNullOrEmpty(path) ? el : el.GetElement(path);
            if (target is not Container c)
                throw new Exception("not a container");
            Addon.NApi.CreateArray(env, out var arr);
            uint i = 0;
            foreach (var child in c.elements) {
                Addon.NApi.CreateBigintUint64(env, Pin(child), out var h);
                Addon.NApi.SetElement(env, arr, i++, h);
            }
            return arr;
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint AddElement(nint env, nint info) =>
        Run(env, info, (env, args) => ResolveOrZero(env, args,
            el => el.AddElement(PathArg(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint FileByName(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var plugin = SessionOf(env, args).pluginManager
                .GetFileByName(Addon.GetString(env, args[1]));
            return plugin == null
                ? Addon.JsBigInt(env, 0)
                : Addon.JsBigInt(env, Pin(plugin));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetRecordByFormId(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var rec = PluginOf(env, args).GetRecordByFormId(
                (uint)Addon.GetNumber(env, args[1]));
            return rec == null
                ? Addon.JsBigInt(env, 0)
                : Addon.JsBigInt(env, Pin(rec));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetValue(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsString(env,
            ElementOf(env, args).GetValue(PathArg(env, args)) ?? ""));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SetValue(nint env, nint info) =>
        Run(env, info, (env, args) => {
            ElementOf(env, args).SetValue(
                Addon.GetString(env, args[1]), Addon.GetString(env, args[2]));
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetIntValue(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsInt(env,
            ElementOf(env, args).GetData<int>(PathArg(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SetIntValue(nint env, nint info) =>
        Run(env, info, (env, args) => {
            ElementOf(env, args).SetData<int>(
                Addon.GetString(env, args[1]), (int)Addon.GetNumber(env, args[2]));
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetUIntValue(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsNumber(env,
            ElementOf(env, args).GetData<uint>(PathArg(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SetUIntValue(nint env, nint info) =>
        Run(env, info, (env, args) => {
            ElementOf(env, args).SetData<uint>(
                Addon.GetString(env, args[1]), (uint)Addon.GetNumber(env, args[2]));
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetFloatValue(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsNumber(env,
            ElementOf(env, args).GetData<float>(PathArg(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SetFloatValue(nint env, nint info) =>
        Run(env, info, (env, args) => {
            ElementOf(env, args).SetData<float>(
                Addon.GetString(env, args[1]), (float)Addon.GetNumber(env, args[2]));
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetSignature(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsString(env, Backend.CleanSig(
            Backend.Safe(() => ElementOf(env, args).signature.ToString()))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetIsModified(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBool(env, ElementOf(env, args)
            .state.HasFlag(ElementState.Modified)));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint DefType(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsInt(env,
            (int)ElementOf(env, args).def.defType));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetContainer(nint env, nint info) =>
        Run(env, info, (env, args) => ResolveOrZero(env, args,
            el => el.container));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint ElementPath(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsString(env,
            Backend.Safe(() => ElementOf(env, args).path)));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint RemoveElement(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBool(env,
            ElementOf(env, args).RemoveElement(PathArg(env, args))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint CopyElement(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var source = ElementOf(env, args);
            var target = ElementOf(env, args, 1);
            var asNew = args.Length > 2 && Addon.GetNumber(env, args[2]) != 0;
            var options = asNew ? CopyOptions.AsNewRecord : default;
            return Addon.JsBigInt(env, Pin(source.CopyTo(target, options)));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint ExchangeReferences(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var el = ElementOf(env, args);
            var oldId = FormId.FromSource(el.file, (uint)Addon.GetBigInt(env, args[1]));
            var newId = FormId.FromSource(el.file, (uint)Addon.GetBigInt(env, args[2]));
            return Addon.JsBool(env, el.ExchangeReferences(oldId, newId));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetRecordFlag(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not MainRecord rec)
                throw new Exception("not a record");
            var flag = Addon.GetString(env, args[1]);
            return Addon.JsBool(env, int.TryParse(flag, out var index)
                ? rec.GetRecordFlag(index)
                : rec.GetRecordFlag(flag));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SetRecordFlag(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not MainRecord rec)
                throw new Exception("not a record");
            var flag = Addon.GetString(env, args[1]);
            var state = args.Length > 2 && Addon.GetNumber(env, args[2]) != 0;
            if (!int.TryParse(flag, out var index)) {
                var unknown = Regex.Match(flag, @"^Unknown (\d+)$");
                if (unknown.Success) {
                    index = int.Parse(unknown.Groups[1].Value);
                } else if (rec.GetElement(@"Record Header\Record Flags") is ValueElement ve
                    && ve.flagsDef != null) {
                    index = ve.flagsDef.GetFlagIndex(flag);
                } else {
                    index = -1;
                }
                if (index < 0) {
                    rec.SetFlag(@"Record Header\Record Flags", flag, state);
                    return Addon.JsBool(env, true);
                }
            }
            rec.SetRecordFlag(index, state);
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetMasterRecord(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not MainRecord rec)
                throw new Exception("not a record");
            var master = rec.master;
            return master == null
                ? Addon.JsBigInt(env, 0)
                : Addon.JsBigInt(env, Pin(master));
        });

    private static MainRecord WinningOverride(MainRecord rec) {
        var overrides = rec.overrides;
        return overrides.Count > 0 ? overrides[^1] : rec;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetWinningOverride(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not MainRecord rec)
                throw new Exception("not a record");
            return Addon.JsBigInt(env, Pin(WinningOverride(rec)));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint IsWinningOverride(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not MainRecord rec)
                throw new Exception("not a record");
            return Addon.JsBool(env, ReferenceEquals(WinningOverride(rec), rec));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetReferencedBy(nint env, nint info) =>
        Run(env, info, (env, args) => {
            if (ElementOf(env, args) is not MainRecord rec)
                throw new Exception("not a record");
            Addon.NApi.CreateArray(env, out var arr);
            uint i = 0;
            foreach (var referrer in rec.referencedBy) {
                Addon.NApi.CreateBigintUint64(env, Pin(referrer), out var h);
                Addon.NApi.SetElement(env, arr, i++, h);
            }
            return arr;
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint BuildReferences(nint env, nint info) =>
        Run(env, info, (env, args) => {
            PluginOf(env, args).BuildRefBy();
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetLinksTo(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var target = ElementOf(env, args).referencedRecord;
            return target == null
                ? Addon.JsBigInt(env, 0)
                : Addon.JsBigInt(env, Pin(target));
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint AddMaster(nint env, nint info) =>
        Run(env, info, (env, args) => {
            PluginOf(env, args).AddMaster(PluginOf(env, args, 1));
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint CleanMasters(nint env, nint info) =>
        Run(env, info, (env, args) => {
            var file = PluginOf(env, args);
            if (file.GetUnusedMasters().Count == 0)
                return Addon.JsBool(env, false);
            file.RemoveUnusedMasters();
            return Addon.JsBool(env, true);
        });

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetIsEditable(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBool(env,
            !ElementOf(env, args).state.HasFlag(ElementState.Protected)));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetElementFlag(nint env, nint info) =>
        Run(env, info, (env, args) => Addon.JsBool(env,
            ElementOf(env, args).GetFlag(
                Addon.GetString(env, args[1]),
                Addon.GetString(env, args[2]))));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint SetElementFlag(nint env, nint info) =>
        Run(env, info, (env, args) => {
            ElementOf(env, args).SetFlag(
                Addon.GetString(env, args[1]),
                Addon.GetString(env, args[2]),
                Addon.GetNumber(env, args[3]) != 0);
            return Addon.JsBool(env, true);
        });
}
