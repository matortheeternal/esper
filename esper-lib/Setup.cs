using esper;
using esper.plugins;
using esper.setup;
using Microsoft.JavaScript.NodeApi;

namespace esper_lib {
    [JSExport]
    public static class Setup {
        private static readonly SessionOptions options = new SessionOptions();

        // INTERNAL API
        internal static Game GetGame(int gameMode) {
            return gameMode switch {
                1 => Games.TES4,
                2 => Games.FO3,
                3 => Games.FNV,
                4 => Games.TES5,
                6 => Games.FO4,
                7 => Games.SSE,
                _ => throw new Exception($"Unknown game mode {gameMode}"),
            };
        }

        // PUBLIC API
        [JSExport]
        public static string GetGamePath(int gameMode) {
            try {
                Game game = GetGame(gameMode);
                return game.GetInstallLocation();
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void SetGamePath(string path) {
            try {
                options.gamePath = path;
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static string GetGameLanguage() {
            try {
                return Meta.session.gameIni.GetConfiguredLanguage();
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void SetLanguage(string language) {
            try {
                options.language = language;
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void SetGameMode(int gameMode) {
            try {
                Meta.session = new Session(GetGame(gameMode), options);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static string GetLoadOrder() {
            try {
                var loadOrder = new LoadOrder(Meta.session);
                return loadOrder.ToJson();
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void LoadPlugins(string loadOrder, bool smartLoad) {
            try {
                LoaderThread.Run(loadOrder, smartLoad);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void LoadPlugin(string filename) {
            try {
                LoaderThread.Run(filename, false);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static uint LoadPluginHeader(string filename) {
            try {
                var file = Meta.session.pluginManager.LoadPluginHeader(filename);
                return Meta.Store(file);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void BuildReferences(uint id, bool sync) {
            try {
                RefThread.Run(id, sync);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static bool UnloadPlugin(uint id) {
            try {
                PluginFile plugin = (PluginFile) Meta.Resolve(id);
                Meta.session.pluginManager.UnloadPlugin(plugin);
                return true;
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static byte GetLoaderStatus() {
            try {
                return (byte)Meta.loaderState;
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }
    }
}
