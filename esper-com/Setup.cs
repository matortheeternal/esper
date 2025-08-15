using esper;
using esper.plugins;
using esper.setup;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace esperlib {
    [Guid("B56FA814-48A7-4552-99E5-D030AB74E916")]
    [ComVisible(true)]
    public interface ISetup {
        unsafe bool GetGamePath(int gameMode, int* len);
        bool SetGamePath(string path);
        unsafe bool GetGameLanguage(int* len);
        bool SetGameMode(int gameMode);
        unsafe bool GetLoadOrder(int* len);
        bool LoadPlugins(string loadOrder, bool smartLoad);
        bool LoadPlugin(string filename);
        unsafe bool LoadPluginHeader(string filename, uint* id);
        bool BuildReferences(uint id, bool sync);
        bool UnloadPlugin(uint id);
        unsafe bool GetLoaderStatus(byte* status);
    }

    [Guid("F3901254-7C95-476B-8586-A68717871FD0")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class Setup : ISetup {
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
        unsafe public bool GetGamePath(int gameMode, int* len) {
            try {
                Game game = GetGame(gameMode);
                Meta.resultStr = game.GetInstallLocation();
                *len = Meta.resultStr.Length;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool SetGamePath(string path) {
            try {
                options.gamePath = path;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        unsafe public bool GetGameLanguage(int* len) {
            try {
                Meta.resultStr = Meta.session.gameIni.GetConfiguredLanguage();
                *len = Meta.resultStr.Length;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool SetLanguage(string language) {
            try {
                options.language = language;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool SetGameMode(int gameMode) {
            try {
                Meta.session = new Session(GetGame(gameMode), options);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        unsafe public bool GetLoadOrder(int* len) {
            try {
                var loadOrder = new LoadOrder(Meta.session);
                Meta.resultStr = loadOrder.ToJson();
                *len = Meta.resultStr.Length;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool LoadPlugins(string loadOrder, bool smartLoad) {
            try {
                LoaderThread.Run(loadOrder, smartLoad);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool LoadPlugin(string filename) {
            try {
                LoaderThread.Run(filename, false);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }


        unsafe public bool LoadPluginHeader(string filename, uint* id) {
            try {
                var file = Meta.session.pluginManager.LoadPluginHeader(filename);
                *id = Meta.Store(file);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool BuildReferences(uint id, bool sync) {
            try {
                RefThread.Run(id, sync);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        public bool UnloadPlugin(uint id) {
            try {
                PluginFile plugin = (PluginFile) Meta.Resolve(id);
                Meta.session.pluginManager.UnloadPlugin(plugin);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        unsafe public bool GetLoaderStatus(byte* status) {
            try {
                *status = (byte)Meta.loaderState;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }
    }
}
