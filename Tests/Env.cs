using System;
using System.IO;

namespace Tests {
    public static class Env {
        private static bool loaded = false;
        private static readonly object loadLock = new object();

        public static void EnsureLoaded() {
            if (loaded) return;
            lock (loadLock) {
                if (loaded) return;
                var dir = new DirectoryInfo(Environment.CurrentDirectory);
                while (dir != null) {
                    var candidate = Path.Combine(dir.FullName, ".env");
                    if (File.Exists(candidate)) {
                        DotNetEnv.Env.Load(candidate);
                        break;
                    }
                    dir = dir.Parent;
                }
                loaded = true;
            }
        }

        public static string Get(string name) {
            var raw = System.Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(raw)) return raw;
            EnsureLoaded();
            return DotNetEnv.Env.GetString(name);
        }

        public static string DataDir(string gameAbbrev) {
            return Get($"ESPER_{gameAbbrev}_DATA");
        }
    }
}
