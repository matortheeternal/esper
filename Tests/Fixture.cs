using esper;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests {
    public static class Fixture {
        public enum Mode {
            Fixture,
            Game,
            Build
        }

        public static readonly Mode mode;

        static Fixture() {
            var raw = Env.Get("ESPER_TEST_MODE") ?? "";
            if (raw.Equals("game", StringComparison.OrdinalIgnoreCase))
                mode = Mode.Game;
            else if (raw.Equals("build", StringComparison.OrdinalIgnoreCase))
                mode = Mode.Build;
            else
                mode = Mode.Fixture;
        }

        public static string EsmPath(Game game, string esm) {
            var dataDir = Env.DataDir(game.abbreviation);
            if (dataDir != null) {
                var candidate = Path.Combine(dataDir, esm);
                if (File.Exists(candidate)) return candidate;
            }
            return TestHelpers.FixturePath(esm);
        }

        public static string FixturePath(string esm) {
            return TestHelpers.FixturePath(Path.Combine("auto", esm));
        }

        private static string SidecarPath(string esm) {
            return Path.Join(TestHelpers.RepoRoot(), "fixtures",
                "auto", Path.GetFileNameWithoutExtension(esm) + ".missing"
            );
        }

        private static string ModePath(Game game, string esm) {
            if (mode == Mode.Game) return EsmPath(game, esm);
            var fixture = FixturePath(esm);
            if (mode == Mode.Build && !File.Exists(fixture))
                return EsmPath(game, esm);
            return fixture;
        }

        private static readonly Dictionary<string, PluginFile> masters =
            new Dictionary<string, PluginFile>();

        private static readonly Dictionary<string, PluginFile> esmFallbacks =
            new Dictionary<string, PluginFile>();

        private static readonly Dictionary<string, Session> esmSessions =
            new Dictionary<string, Session>();

        private static PluginFile LoadCached(
            Dictionary<string, PluginFile> cache,
            Session session,
            string key,
            string path,
            string display
        ) {
            lock (cache) {
                if (!cache.TryGetValue(key, out var plugin)) {
                    if (!File.Exists(path)) Assert.Ignore($"{display} missing.");
                    plugin = session.pluginManager.LoadPlugin(path);
                    cache[key] = plugin;
                }
                return plugin;
            }
        }

        public static PluginFile Master(Game game, string esm) {
            var path = ModePath(game, esm);
            var key = game.abbreviation + ":" + path;
            return LoadCached(
                masters, Synthetic.GetSession(game), key, path, esm
            );
        }

        public static PluginFile LoadFresh(Game game, string esm) {
            var path = ModePath(game, esm);
            if (!File.Exists(path)) Assert.Ignore($"{esm} missing.");
            return Synthetic.GetSession(game).pluginManager.LoadPlugin(path);
        }

        private static PluginFile BuildEsm(Game game, string esm) {
            var path = EsmPath(game, esm);
            var key = game.abbreviation + ":" + path;
            lock (esmSessions) {
                if (!esmSessions.TryGetValue(game.abbreviation, out var session)) {
                    session = new Session(game, new SessionOptions());
                    esmSessions[game.abbreviation] = session;
                }
                return LoadCached(esmFallbacks, session, key, path, esm);
            }
        }

        private static void AppendSidecar(string esm, uint localId) {
            var path = SidecarPath(esm);
            HashSet<string> ids;
            if (File.Exists(path))
                ids = new HashSet<string>(File.ReadAllLines(path));
            else
                ids = new HashSet<string>();
            var entry = localId.ToString("X8");
            if (!ids.Contains(entry)) {
                ids.Add(entry);
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllLines(path, ids.OrderBy(x => x));
            }
        }

        public static MainRecord Record(Game game, string esm, uint localId) {
            var master = Master(game, esm);
            if (master == null) Assert.Ignore($"{esm} missing.");
            var rec = master.GetRecordByFormId(localId)
                ?? master.GetRecordByFormId(localId & 0xFFFFFFu);
            if (rec != null) return rec;
            if (mode == Mode.Build) {
                AppendSidecar(esm, localId & 0xFFFFFFu);
                var fallback = BuildEsm(game, esm);
                if (fallback == null) Assert.Ignore($"{esm} missing.");
                return fallback.GetRecordByFormId(localId);
            }
            if (mode == Mode.Game) return null;
            Assert.Fail(
                $"Record 0x{localId:X} not in fixture {esm}; " +
                "run with ESPER_TEST_MODE=build to extend it."
            );
            return null;
        }

        public static void Require(Game game, string esm, params uint[] localIds) {
            if (mode == Mode.Build) {
                var master = Master(game, esm);
                if (master == null) Assert.Ignore($"{esm} missing.");
                foreach (var id in localIds)
                    if (master.GetRecordByFormId(id) == null
                        && master.GetRecordByFormId(id & 0xFFFFFFu) == null)
                        AppendSidecar(esm, id & 0xFFFFFFu);
                return;
            }
            var target = Master(game, esm);
            if (target == null) Assert.Ignore($"{esm} missing.");
            var missing = localIds
                .Where(id => target.GetRecordByFormId(id) == null
                    && target.GetRecordByFormId(id & 0xFFFFFFu) == null)
                .Select(id => $"0x{id:X}")
                .ToList();
            if (missing.Count > 0) {
                if (mode == Mode.Game)
                    Assert.Fail($"Records missing from {esm}: {string.Join(", ", missing)}.");
                Assert.Fail(
                    $"Records missing from fixture {esm}: {string.Join(", ", missing)}. " +
                    "Run with ESPER_TEST_MODE=build to extend it."
                );
            }
        }

        public static List<MainRecord> ScanDistinct(
            Game game,
            string esm,
            IEnumerable<MainRecord> records,
            Func<MainRecord, string> key
        ) {
            var seen = new HashSet<string>();
            var kept = new List<MainRecord>();
            TestHelpers.ScanRecords(records, rec => {
                var k = key(rec);
                if (k == null || !seen.Add(k)) return;
                kept.Add(rec);
                if (mode == Mode.Build)
                    AppendSidecar(esm, rec.fileFormId & 0xFFFFFFu);
            });
            return kept;
        }

        public static List<string> StringFiles(Game game, string esm) {
            var found = new List<string>();
            if (mode != Mode.Game) {
                var stem = TestHelpers.FixturePath(Path.Combine(
                    "auto",
                    Path.GetFileNameWithoutExtension(esm) + "_english"
                ));
                foreach (var ext in new[] { ".strings", ".dlstrings", ".ilstrings" })
                    if (File.Exists(stem + ext)) found.Add(stem + ext);
            } else if (game.abbreviation == "SSE") {
                var dir = TestHelpers.FixturePath("skyrim_strings");
                foreach (var name in new[] {
                    "skyrim_english.strings",
                    "skyrim_english.dlstrings",
                    "skyrim_english.ilstrings"
                }) {
                    var path = Path.Combine(dir, name);
                    if (File.Exists(path)) found.Add(path);
                }
            }
            return found;
        }

        public static void LoadStrings(Game game, PluginFile plugin, string esm) {
            var files = StringFiles(game, esm);
            if (files.Count == 0) return;
            Synthetic.GetSession(game).resourceManager.LoadPluginStrings(plugin, files);
        }
    }
}
