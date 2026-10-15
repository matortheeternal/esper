using System.Text;
using System.Text.RegularExpressions;
using esper;
using esper.elements;
using esper.plugins;
using esper.setup;

internal static class Backend {
    internal sealed record GameEntry(
        string Key, string Name, string FullName, string EsmName,
        string EnvVar, int[] AppIds, string[] DirNames, Func<Game> Make);

    internal static readonly GameEntry[] Games = {
        Entry("TES4", "Oblivion", "The Elder Scrolls IV: Oblivion", "Oblivion.esm",
            "ESPER_TES4_DATA", new[]{ 22330, 900883 }, new[]{ "Oblivion" }, () => esper.Games.TES4),
        Entry("FO3", "Fallout 3", "Fallout 3", "Fallout3.esm",
            "ESPER_FO3_DATA", new[]{ 22300, 22370 }, new[]{ "Fallout 3" }, () => esper.Games.FO3),
        Entry("FNV", "Fallout NV", "Fallout: New Vegas", "FalloutNV.esm",
            "ESPER_FNV_DATA", new[]{ 22380, 2028016 }, new[]{ "Fallout New Vegas" }, () => esper.Games.FNV),
        Entry("TES5", "Skyrim", "The Elder Scrolls V: Skyrim", "Skyrim.esm",
            "ESPER_TES5_DATA", new[]{ 72850 }, new[]{ "Skyrim" }, () => esper.Games.TES5),
        Entry("SSE", "Skyrim SE", "Skyrim: Special Edition", "Skyrim.esm",
            "ESPER_SSE_DATA", new[]{ 489830 }, new[]{ "Skyrim Special Edition" }, () => esper.Games.SSE),
        Entry("FO4", "Fallout 4", "Fallout 4", "Fallout4.esm",
            "ESPER_FO4_DATA", new[]{ 377160 }, new[]{ "Fallout 4" }, () => esper.Games.FO4),
        Entry("SF", "Starfield", "Starfield", "Starfield.esm",
            "ESPER_SF_DATA", new[]{ 1716740 }, new[]{ "Starfield" }, () => esper.Games.SF),
    };

    private static GameEntry Entry(
        string key, string name, string full, string esm, string env,
        int[] appIds, string[] dirs, Func<Game> make
    ) => new(key, name, full, esm, env, appIds, dirs, make);

    internal static Session OpenSession(string gameKey, string gamePath) {
        var entry = Games.FirstOrDefault(g =>
            g.Key.Equals(gameKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception($"unknown gameKey: {gameKey}");
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            throw new Exception($"gamePath not found: {gamePath}");
        return new Session(entry.Make(), new SessionOptions {
            gamePath = gamePath,
            loadResources = true,
        });
    }

    internal static string DetectGames() {
        var libraries = LibraryRoots();
        var appIds = libraries.SelectMany(InstalledAppIds).ToHashSet();
        var rows = Games.Select(entry => {
            var root = Environment.GetEnvironmentVariable(entry.EnvVar);
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) root = null;
            root ??= libraries
                .SelectMany(lib => entry.DirNames.Select(dir =>
                    Path.Combine(lib, "steamapps/common", dir)))
                .FirstOrDefault(Directory.Exists);
            var dataPath = root == null ? "" : Path.Combine(root, "Data");
            return "{" + Json.Prop("key", entry.Key)
                + "," + Json.Prop("name", entry.Name)
                + "," + Json.Prop("fullName", entry.FullName)
                + "," + Json.Prop("esmName", entry.EsmName)
                + "," + Json.Prop("gamePath", root ?? "")
                + "," + Json.Prop("dataPath", dataPath)
                + "," + Json.Raw("detected", root != null ? "true" : "false")
                + "," + Json.Raw("viaSteam", entry.AppIds.Any(appIds.Contains) ? "true" : "false")
                + "," + Json.Raw("esmPresent", dataPath != "" &&
                    File.Exists(Path.Combine(dataPath, entry.EsmName)) ? "true" : "false") + "}";
        });
        return "[" + string.Join(",", rows) + "]";
    }

    private static IEnumerable<string> LibraryRoots() {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var roots = new List<string> {
            Path.Combine(home, ".steam/steam"),
            "/mnt/elements/SteamLibrary",
        };
        foreach (var vdf in new[] {
            Path.Combine(home, ".steam/steam/steamapps/libraryfolders.vdf"),
            Path.Combine(home, ".local/share/Steam/steamapps/libraryfolders.vdf"),
        }) {
            if (!File.Exists(vdf)) continue;
            roots.AddRange(Regex.Matches(File.ReadAllText(vdf), @"""path""\s+""([^""]+)""")
                .Select(m => m.Groups[1].Value.Replace(@"\\", "/")));
        }
        return roots.Where(Directory.Exists).Distinct();
    }

    private static HashSet<int> InstalledAppIds(string library) {
        var found = new HashSet<int>();
        var dir = Path.Combine(library, "steamapps");
        if (!Directory.Exists(dir)) return found;
        foreach (var acf in Directory.EnumerateFiles(dir, "appmanifest_*.acf"))
            foreach (Match m in Regex.Matches(File.ReadAllText(acf), @"""appid""\s+""(\d+)"""))
                found.Add(int.Parse(m.Groups[1].Value));
        return found;
    }

    internal static string LoadOrder(Session session) =>
        "[" + string.Join(",", new LoadOrder(session).modules.Select(m =>
            "{" + Json.Prop("name", m.name)
            + "," + Json.Raw("active", m.flags.HasFlag(ModuleFlags.Active) ? "true" : "false")
            + "," + Json.Raw("missing", m.flags.HasFlag(ModuleFlags.MastersMissing) ? "true" : "false")
            + "," + Json.Raw("masters", Json.List(m.masterNames ?? new List<string>())) + "}")) + "]";

    internal static string KindOf(Element el) {
        if (el is PluginFile) return "file";
        if (el is GroupRecord) return "group";
        if (el is MainRecord) return "record";
        if (el is ValueElement) return "value";
        if (el is Container) return "struct";
        return "element";
    }

    internal static string LabelOf(Element el) => KindOf(el) switch {
        "file" => ((PluginFile)el).name,
        "record" => Safe(() => {
            var rec = (MainRecord)el;
            return $"{rec.signature} {rec.fileFormId:X8} {Safe(() => rec.editorId)}".Trim();
        }),
        "value" => Safe(() => $"{el.displayName}: {((ValueElement)el).value}"),
        _ => Safe(() => el.displayName),
    };

    internal static string Safe(Func<string> read) {
        try { return read() ?? ""; } catch { return ""; }
    }

    internal static bool SafeFlag(Func<bool> read) {
        try { return read(); } catch { return false; }
    }

    internal static string CleanSig(string sig) =>
        string.IsNullOrEmpty(sig) || sig.Contains('\0') ? "" : sig;

    internal static bool IsModified(PluginFile plugin) {
        bool flagged(Element el) =>
            el != null && el.state.HasFlag(ElementState.Modified);
        return flagged(plugin) || flagged(plugin.header) ||
            (plugin.records?.Any(flagged) ?? false);
    }
}

internal static class Json {
    internal static string Quote(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"")
            .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";

    internal static string Prop(string name, string value) =>
        Quote(name) + ":" + Quote(value);

    internal static string Raw(string name, string json) =>
        Quote(name) + ":" + json;

    internal static string List(IEnumerable<string> items) =>
        "[" + string.Join(",", items.Select(Quote)) + "]";
}
